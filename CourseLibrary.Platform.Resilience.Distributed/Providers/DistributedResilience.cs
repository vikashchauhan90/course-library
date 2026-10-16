using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using CourseLibrary.Platform.Resilience.Distributed.Abstractions;
using CourseLibrary.Platform.Resilience.Distributed.Observability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace CourseLibrary.Platform.Resilience.Distributed.Providers;

internal sealed class DistributedResilience(
    IDistributedResilienceStore store,
    IOptions<DistributedResilienceOptions> options,
    ILogger<DistributedResilience> logger) : IDistributedResilience
{
    private readonly IDistributedResilienceStore _store = store;
    private readonly DistributedResilienceOptions _options = options.Value;
    private readonly ILogger<DistributedResilience> _logger = logger;
    private readonly ConcurrentDictionary<DistributedCircuitKey, PipelineCacheEntry> _pipelines = new();

    public async ValueTask ExecuteAsync(
        DistributedCircuitKey key,
        Func<CancellationToken, ValueTask> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await ExecuteAsync<object?>(
            key,
            async token =>
            {
                await operation(token).ConfigureAwait(false);
                return null;
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<TResult> ExecuteAsync<TResult>(
        DistributedCircuitKey key,
        Func<CancellationToken, ValueTask<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(key.Dependency);
        ArgumentException.ThrowIfNullOrWhiteSpace(key.Operation);

        using var activity = DistributedResilienceActivitySource.Shared.StartActivity(
            "DistributedResilience.ExecuteAsync");
        SetActivityTags(activity, key, "execute");
        PipelineCacheEntry? pipelineEntry = null;

        try
        {
            var status = await _store.GetStateAsync(key, cancellationToken).ConfigureAwait(false);
            if (status.State == DistributedCircuitState.Open)
            {
                throw CreateBlockedCall(activity, key, status.OpenUntil);
            }

            if (status.State == DistributedCircuitState.HalfOpen)
            {
                var lease = await _store.TryAcquireProbeAsync(
                    key,
                    _options.ProbeLeaseDuration,
                    cancellationToken).ConfigureAwait(false);
                if (lease is null)
                {
                    throw CreateBlockedCall(activity, key, status.OpenUntil);
                }

                var probeResult = await ExecuteProbeAsync(
                    key,
                    lease.Value,
                    operation,
                    cancellationToken).ConfigureAwait(false);
                activity?.SetTag(DistributedResilienceActivityTagConstants.Outcome, "success");
                DistributedResilienceLogs.PipelineExecutionSucceeded(
                    _logger,
                    key.Dependency,
                    key.Operation);
                DistributedResilienceMeter.Shared.RecordOperation("execute", "success");
                return probeResult;
            }

            pipelineEntry = GetPipeline(key);
            await CloseStaleLocalBreakersAsync(key).ConfigureAwait(false);
            TResult result;
            try
            {
                result = await pipelineEntry.Pipeline.ExecuteAsync(
                    async token =>
                    {
                        var value = await operation(token).ConfigureAwait(false);
                        if (value is HttpResponseMessage response && IsHandledResponse(response))
                        {
                            throw new RetriableHttpResponseException(response);
                        }

                        return value;
                    },
                    cancellationToken).ConfigureAwait(false);
            }
            catch (RetriableHttpResponseException exception)
            {
                await PublishLocalBreakerStateAsync(key, pipelineEntry).ConfigureAwait(false);
                activity?.SetTag(DistributedResilienceActivityTagConstants.Outcome, "http-response");
                DistributedResilienceMeter.Shared.RecordOperation("execute", "http-response");
                return (TResult)(object)exception.Response;
            }

            await PublishLocalBreakerStateAsync(key, pipelineEntry).ConfigureAwait(false);
            activity?.SetTag(DistributedResilienceActivityTagConstants.Outcome, "success");
            DistributedResilienceLogs.PipelineExecutionSucceeded(
                _logger,
                key.Dependency,
                key.Operation);
            DistributedResilienceMeter.Shared.RecordOperation("execute", "success");
            return result;
        }
        catch (OperationCanceledException)
        {
            if (pipelineEntry is not null)
            {
                await PublishLocalBreakerStateAsync(key, pipelineEntry).ConfigureAwait(false);
            }

            RecordOutcome(activity, "cancelled");
            DistributedResilienceLogs.PipelineExecutionCancelled(
                _logger,
                key.Dependency,
                key.Operation);
            throw;
        }
        catch (DistributedCircuitOpenException)
        {
            RecordOutcome(activity, "blocked");
            throw;
        }
        catch (Exception exception)
        {
            if (pipelineEntry is not null)
            {
                await PublishLocalBreakerStateAsync(key, pipelineEntry).ConfigureAwait(false);
            }

            RecordOutcome(activity, "error");
            DistributedResilienceLogs.PipelineExecutionFailed(
                _logger,
                exception,
                key.Dependency,
                key.Operation);
            throw;
        }
    }

    private async ValueTask<TResult> ExecuteProbeAsync<TResult>(
        DistributedCircuitKey key,
        DistributedProbeLease lease,
        Func<CancellationToken, ValueTask<TResult>> operation,
        CancellationToken cancellationToken)
    {
        TResult result;
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(_options.Timeout);

        try
        {
            result = await operation(timeoutSource.Token).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            var failed = exception is OperationCanceledException ||
                IsHandledException(exception);
            var completed = await _store.CompleteProbeAsync(
                key,
                lease,
                succeeded: !failed,
                GetProbeFailureDuration<TResult>(default),
                CancellationToken.None).ConfigureAwait(false);
            if (completed && !failed)
            {
                await CloseLocalBreakersAsync(key).ConfigureAwait(false);
            }

            throw;
        }

        var failedResult = result is HttpResponseMessage response &&
            IsHandledResponse(response);
        var leaseCompleted = await _store.CompleteProbeAsync(
            key,
            lease,
            succeeded: !failedResult,
            GetProbeFailureDuration(result),
            CancellationToken.None).ConfigureAwait(false);
        if (leaseCompleted && !failedResult)
        {
            await CloseLocalBreakersAsync(key).ConfigureAwait(false);
        }

        return result;
    }

    private PipelineCacheEntry GetPipeline(DistributedCircuitKey key)
    {
        return _pipelines.GetOrAdd(key, CreatePipeline);
    }

    private PipelineCacheEntry CreatePipeline(DistributedCircuitKey key)
    {
        var stateProvider = new CircuitBreakerStateProvider();
        var manualControl = new CircuitBreakerManualControl();
        var builder = new ResiliencePipelineBuilder();

        builder.AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = _options.MaxRetryAttempts,
            Delay = _options.RetryDelay,
            MaxDelay = _options.MaxRetryDelay,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = args => new ValueTask<bool>(IsHandledException(args.Outcome.Exception)),
            DelayGenerator = args => new ValueTask<TimeSpan?>(GetRetryAfter(args.Outcome.Exception)),
            OnRetry = args =>
            {
                if (args.Outcome.Exception is RetriableHttpResponseException responseException)
                {
                    responseException.Response.Dispose();
                }

                DistributedResilienceLogs.RetryAttempted(
                    _logger,
                    key.Dependency,
                    key.Operation,
                    args.AttemptNumber + 1,
                    args.RetryDelay);
                return default;
            }
        });

        builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = _options.FailureRatio,
            MinimumThroughput = _options.MinimumThroughput,
            SamplingDuration = _options.SamplingDuration,
            BreakDuration = _options.BreakDuration,
            StateProvider = stateProvider,
            ManualControl = manualControl,
            ShouldHandle = args => new ValueTask<bool>(IsHandledException(args.Outcome.Exception))
        });

        builder.AddTimeout(_options.Timeout);

        return new PipelineCacheEntry(builder.Build(), stateProvider, manualControl);
    }

    private async ValueTask PublishLocalBreakerStateAsync(
        DistributedCircuitKey key,
        PipelineCacheEntry pipeline)
    {
        if (pipeline.StateProvider.CircuitState == CircuitState.Open)
        {
            await _store.OpenAsync(key, _options.BreakDuration).ConfigureAwait(false);
        }
    }

    private ValueTask CloseStaleLocalBreakersAsync(DistributedCircuitKey key) =>
        CloseLocalBreakersAsync(key);

    private async ValueTask CloseLocalBreakersAsync(DistributedCircuitKey key)
    {
        foreach (var entry in _pipelines)
        {
            if (entry.Key == key &&
                entry.Value.StateProvider.CircuitState != CircuitState.Closed)
            {
                await entry.Value.ManualControl.CloseAsync().ConfigureAwait(false);
            }
        }
    }

    private DistributedCircuitOpenException CreateBlockedCall(
        Activity? activity,
        DistributedCircuitKey key,
        DateTimeOffset? openUntil)
    {
        activity?.SetTag(DistributedResilienceActivityTagConstants.State, "open");
        activity?.SetTag(DistributedResilienceActivityTagConstants.Outcome, "blocked");
        DistributedResilienceLogs.CircuitCallBlocked(
            _logger,
            key.Dependency,
            key.Operation,
            openUntil);
        return new DistributedCircuitOpenException(key, openUntil);
    }

    private static bool IsHandledException(Exception? exception)
    {
        return exception is HttpRequestException or TimeoutRejectedException or TimeoutException or
            RetriableHttpResponseException;
    }

    private static bool IsHandledResponse(HttpResponseMessage response) =>
        response.StatusCode == HttpStatusCode.RequestTimeout ||
        response.StatusCode == HttpStatusCode.TooManyRequests ||
        (int)response.StatusCode >= (int)HttpStatusCode.InternalServerError;

    private TimeSpan? GetRetryAfter(Exception? exception)
    {
        if (exception is not RetriableHttpResponseException responseException ||
            responseException.Response.StatusCode != HttpStatusCode.TooManyRequests)
        {
            return null;
        }

        var retryAfter = responseException.Response.Headers.RetryAfter;
        var delay = retryAfter?.Delta;
        if (delay is null && retryAfter?.Date is DateTimeOffset date)
        {
            delay = date - DateTimeOffset.UtcNow;
        }

        if (delay is null || delay < TimeSpan.Zero)
        {
            return null;
        }

        return delay > _options.MaxRetryDelay ? _options.MaxRetryDelay : delay;
    }

    private TimeSpan GetProbeFailureDuration<TResult>(TResult? result)
    {
        if (result is HttpResponseMessage response && response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var delay = response.Headers.RetryAfter?.Delta;
            if (delay is null && response.Headers.RetryAfter?.Date is DateTimeOffset date)
            {
                delay = date - DateTimeOffset.UtcNow;
            }

            if (delay.HasValue && delay.Value >= TimeSpan.Zero)
            {
                return delay > _options.MaxRetryDelay ? _options.MaxRetryDelay : delay.Value;
            }
        }

        return _options.BreakDuration;
    }

    private void RecordOutcome(Activity? activity, string outcome)
    {
        activity?.SetTag(DistributedResilienceActivityTagConstants.Outcome, outcome);
        activity?.SetStatus(
            outcome == "error" || outcome == "cancelled"
                ? ActivityStatusCode.Error
                : ActivityStatusCode.Ok,
            outcome);
        DistributedResilienceMeter.Shared.RecordOperation("execute", outcome);
    }

    private static void SetActivityTags(
        Activity? activity,
        DistributedCircuitKey key,
        string operation)
    {
        activity?.SetTag(DistributedResilienceActivityTagConstants.Dependency, key.Dependency);
        activity?.SetTag(DistributedResilienceActivityTagConstants.Operation, key.Operation);
        activity?.SetTag(DistributedResilienceActivityTagConstants.OperationType, operation);
    }

    private sealed record PipelineCacheEntry(
        ResiliencePipeline Pipeline,
        CircuitBreakerStateProvider StateProvider,
        CircuitBreakerManualControl ManualControl);

    private sealed class RetriableHttpResponseException(HttpResponseMessage response)
        : Exception($"Downstream returned retryable HTTP status {(int)response.StatusCode}.")
    {
        public HttpResponseMessage Response { get; } = response;
    }
}
