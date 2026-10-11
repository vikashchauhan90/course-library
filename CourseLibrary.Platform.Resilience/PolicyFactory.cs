using CourseLibrary.Platform.Observability.Traces;
using CourseLibrary.Platform.Resilience.Abstractions;
using CourseLibrary.Platform.Resilience.Observability;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using System.Diagnostics;

namespace CourseLibrary.Platform.Resilience;

internal sealed class PolicyFactory(
    ResiliencePipelineProvider<string> provider,
    ILogger<PolicyFactory> logger) : IPolicyFactory
{
    private static readonly PlatformActivitySource ActivitySources = new();
    private static readonly ResilienceMeter Metrics = ResilienceMeter.Shared;

    public ResiliencePipeline Get(string policyName)
    {
        using var activity = ActivitySources.StartActivity("PolicyFactory.Get");
        activity?.SetTag(ResilienceActivityTagConstants.PolicyName, policyName);
        activity?.SetTag(
            ResilienceActivityTagConstants.Operation,
            ResilienceActivityTagConstants.GetPipelineOperation);

        try
        {
            var pipeline = provider.GetPipeline(policyName);
            activity?.SetTag(ResilienceActivityTagConstants.Outcome, ResilienceActivityTagConstants.SuccessOutcome);
            Metrics.RecordOperation(
                ResilienceActivityTagConstants.GetPipelineOperation,
                ResilienceActivityTagConstants.SuccessOutcome);
            logger.PipelineRetrieved(policyName);
            return pipeline;
        }
        catch (Exception exception)
        {
            activity?.SetTag(ResilienceActivityTagConstants.Outcome, ResilienceActivityTagConstants.ErrorOutcome);
            activity?.SetStatus(ActivityStatusCode.Error, ResilienceActivityTagConstants.ErrorOutcome);
            Metrics.RecordOperation(
                ResilienceActivityTagConstants.GetPipelineOperation,
                ResilienceActivityTagConstants.ErrorOutcome);
            logger.PipelineRetrievalFailed(exception, policyName);
            throw;
        }
    }

    public async ValueTask ExecuteAsync(
        string policyName,
        Func<CancellationToken, ValueTask> operation,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.StartActivity(
           "PolicyFactory.ExecuteAsync");
        activity?.SetTag(ResilienceActivityTagConstants.PolicyName, policyName);
        activity?.SetTag(ResilienceActivityTagConstants.OperationType, "void");
        activity?.SetTag(ResilienceActivityTagConstants.Operation, ResilienceActivityTagConstants.ExecuteOperation);

        try
        {
            var pipeline = provider.GetPipeline(policyName);
            await pipeline.ExecuteAsync(operation, cancellationToken);
            RecordOutcome(activity, policyName, ResilienceActivityTagConstants.SuccessOutcome);
        }
        catch (OperationCanceledException)
        {
            RecordOutcome(activity, policyName, ResilienceActivityTagConstants.CancelledOutcome);
            logger.ExecutionCancelled(policyName);
            throw;
        }
        catch (Exception exception)
        {
            RecordOutcome(activity, policyName, ResilienceActivityTagConstants.ErrorOutcome);
            logger.ExecutionFailed(exception, policyName);
            throw;
        }
    }

    public async ValueTask<T> ExecuteAsync<T>(
        string policyName,
        Func<CancellationToken, ValueTask<T>> operation,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.StartActivity(
            "PolicyFactory.ExecuteAsync<T>");
        activity?.SetTag(ResilienceActivityTagConstants.PolicyName, policyName);
        activity?.SetTag(ResilienceActivityTagConstants.OperationType, "result");
        activity?.SetTag(ResilienceActivityTagConstants.Operation, ResilienceActivityTagConstants.ExecuteOperation);

        try
        {
            var pipeline = provider.GetPipeline(policyName);
            var result = await pipeline.ExecuteAsync(operation, cancellationToken);
            RecordOutcome(activity, policyName, ResilienceActivityTagConstants.SuccessOutcome);
            return result;
        }
        catch (OperationCanceledException)
        {
            RecordOutcome(activity, policyName, ResilienceActivityTagConstants.CancelledOutcome);
            logger.ExecutionCancelled(policyName);
            throw;
        }
        catch (Exception exception)
        {
            RecordOutcome(activity, policyName, ResilienceActivityTagConstants.ErrorOutcome);
            logger.ExecutionFailed(exception, policyName);
            throw;
        }
    }

    private void RecordOutcome(Activity? activity, string policyName, string outcome)
    {
        activity?.SetTag(ResilienceActivityTagConstants.Outcome, outcome);
        if (outcome == ResilienceActivityTagConstants.ErrorOutcome ||
            outcome == ResilienceActivityTagConstants.CancelledOutcome)
        {
            activity?.SetStatus(ActivityStatusCode.Error, outcome);
        }
        else
        {
            logger.ExecutionSucceeded(policyName);
        }

        Metrics.RecordOperation(ResilienceActivityTagConstants.ExecuteOperation, outcome);
    }
}
