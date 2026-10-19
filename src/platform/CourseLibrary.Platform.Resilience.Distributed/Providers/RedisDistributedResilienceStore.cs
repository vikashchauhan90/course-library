using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CourseLibrary.Platform.Resilience.Distributed.Abstractions;
using CourseLibrary.Platform.Resilience.Distributed.Observability;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CourseLibrary.Platform.Resilience.Distributed.Providers;

internal sealed class RedisDistributedResilienceStore(
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<DistributedResilienceOptions> options,
    ILogger<RedisDistributedResilienceStore> logger) : IDistributedResilienceStore
{
    private const int StateUpdateRetries = 5;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;
    private readonly DistributedResilienceOptions _options = GetValidatedOptions(options);
    private readonly ILogger<RedisDistributedResilienceStore> _logger = logger;

    public async ValueTask<DistributedCircuitStatus> GetStateAsync(
        DistributedCircuitKey key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var (stateKey, probeKey) = CreateRedisKeys(key);
        using var activity = DistributedResilienceActivitySource.Shared.StartActivity(
            "RedisDistributedResilienceStore.GetState");
        SetActivityTags(activity, key, "get-state");

        try
        {
            var database = GetDatabase();
            var stateValue = await database.StringGetAsync(stateKey).ConfigureAwait(false);
            var stateRecord = DeserializeState(stateValue);
            var state = GetCircuitState(stateRecord);
            var probeInProgress = stateRecord is not null &&
                await database.KeyExistsAsync(probeKey).ConfigureAwait(false);
            var status = new DistributedCircuitStatus(
                state,
                stateRecord?.OpenUntil,
                probeInProgress);

            activity?.SetTag(DistributedResilienceActivityTagConstants.State, state.ToString());
            DistributedResilienceLogs.CircuitStateRead(_logger, key.Dependency, key.Operation, state.ToString());
            DistributedResilienceMeter.Shared.RecordOperation("get-state", "success");
            return status;
        }
        catch (Exception exception)
        {
            RecordFailure(activity, exception, key, "get-state");
            throw;
        }
    }

    public async ValueTask OpenAsync(
        DistributedCircuitKey key,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        ValidateDuration(duration, nameof(duration));
        cancellationToken.ThrowIfCancellationRequested();
        var (stateKey, probeKey) = CreateRedisKeys(key);
        using var activity = DistributedResilienceActivitySource.Shared.StartActivity(
            "RedisDistributedResilienceStore.Open");
        SetActivityTags(activity, key, "open");

        try
        {
            var database = GetDatabase();
            var requestedOpenUntil = DateTimeOffset.UtcNow.AddMilliseconds(ToMilliseconds(duration));

            for (var attempt = 0; attempt < StateUpdateRetries; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var currentValue = await database.StringGetAsync(stateKey).ConfigureAwait(false);
                var currentRecord = DeserializeState(currentValue);
                var openUntil = currentRecord is not null && currentRecord.OpenUntil > requestedOpenUntil
                    ? currentRecord.OpenUntil
                    : requestedOpenUntil;
                var stateRecord = new DistributedCircuitRecord(openUntil);
                var stateExpiry = openUntil - DateTimeOffset.UtcNow + _options.StateRetention;
                if (stateExpiry <= TimeSpan.Zero)
                {
                    stateExpiry = _options.StateRetention;
                }

                var transaction = database.CreateTransaction();
                AddStateCondition(transaction, stateKey, currentValue);
                _ = transaction.StringSetAsync(
                    stateKey,
                    SerializeState(stateRecord),
                    stateExpiry);
                _ = transaction.KeyDeleteAsync(probeKey);

                if (await transaction.ExecuteAsync().ConfigureAwait(false))
                {
                    activity?.SetTag(
                        DistributedResilienceActivityTagConstants.State,
                        DistributedCircuitState.Open.ToString());
                    DistributedResilienceLogs.CircuitOpened(_logger, key.Dependency, key.Operation, duration);
                    DistributedResilienceMeter.Shared.RecordOperation("open", "success");
                    return;
                }
            }

            throw new InvalidOperationException(
                $"Could not update the distributed circuit state for {key.Dependency}/{key.Operation} after {StateUpdateRetries} attempts.");
        }
        catch (Exception exception)
        {
            RecordFailure(activity, exception, key, "open");
            throw;
        }
    }

    public async ValueTask<DistributedProbeLease?> TryAcquireProbeAsync(
        DistributedCircuitKey key,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        ValidateDuration(leaseDuration, nameof(leaseDuration));
        cancellationToken.ThrowIfCancellationRequested();
        var (stateKey, probeKey) = CreateRedisKeys(key);
        var token = Guid.NewGuid().ToString("N");
        using var activity = DistributedResilienceActivitySource.Shared.StartActivity(
            "RedisDistributedResilienceStore.TryAcquireProbe");
        SetActivityTags(activity, key, "try-acquire-probe");

        try
        {
            var database = GetDatabase();
            var stateValue = await database.StringGetAsync(stateKey).ConfigureAwait(false);
            var stateRecord = DeserializeState(stateValue);
            if (stateRecord is null ||
                stateRecord.OpenUntil > DateTimeOffset.UtcNow)
            {
                return RecordProbeAcquisition(activity, key, acquired: false, token);
            }

            var transaction = database.CreateTransaction();
            transaction.AddCondition(Condition.StringEqual(stateKey, stateValue));
            var acquireTask = transaction.StringSetAsync(
                probeKey,
                token,
                leaseDuration,
                When.NotExists);
            var transactionCompleted = await transaction.ExecuteAsync().ConfigureAwait(false);
            var acquired = transactionCompleted &&
                await acquireTask.ConfigureAwait(false);

            return RecordProbeAcquisition(activity, key, acquired, token);
        }
        catch (Exception exception)
        {
            RecordFailure(activity, exception, key, "try-acquire-probe");
            throw;
        }
    }

    public async ValueTask<bool> CompleteProbeAsync(
        DistributedCircuitKey key,
        DistributedProbeLease lease,
        bool succeeded,
        TimeSpan openDurationOnFailure,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lease.Token);
        if (!succeeded)
        {
            ValidateDuration(openDurationOnFailure, nameof(openDurationOnFailure));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var (stateKey, probeKey) = CreateRedisKeys(key);
        using var activity = DistributedResilienceActivitySource.Shared.StartActivity(
            "RedisDistributedResilienceStore.CompleteProbe");
        SetActivityTags(activity, key, "complete-probe");

        try
        {
            var database = GetDatabase();
            var transaction = database.CreateTransaction();
            transaction.AddCondition(Condition.StringEqual(probeKey, lease.Token));

            if (succeeded)
            {
                _ = transaction.KeyDeleteAsync(stateKey);
            }
            else
            {
                var openUntil = DateTimeOffset.UtcNow.AddMilliseconds(
                    ToMilliseconds(openDurationOnFailure));
                var stateRecord = new DistributedCircuitRecord(openUntil);
                var stateExpiry = openDurationOnFailure + _options.StateRetention;
                _ = transaction.StringSetAsync(
                    stateKey,
                    SerializeState(stateRecord),
                    stateExpiry);
            }

            _ = transaction.KeyDeleteAsync(probeKey);
            var completed = await transaction.ExecuteAsync().ConfigureAwait(false);

            activity?.SetTag(DistributedResilienceActivityTagConstants.ProbeSucceeded, succeeded);
            activity?.SetTag(DistributedResilienceActivityTagConstants.LeaseCompleted, completed);
            DistributedResilienceLogs.ProbeCompleted(
                _logger,
                key.Dependency,
                key.Operation,
                succeeded,
                completed);
            DistributedResilienceMeter.Shared.RecordOperation(
                "complete-probe",
                completed ? (succeeded ? "success" : "reopened") : "lease-lost");
            return completed;
        }
        catch (Exception exception)
        {
            RecordFailure(activity, exception, key, "complete-probe");
            throw;
        }
    }

    private DistributedProbeLease? RecordProbeAcquisition(
        Activity? activity,
        DistributedCircuitKey key,
        bool acquired,
        string token)
    {
        activity?.SetTag(DistributedResilienceActivityTagConstants.ProbeAcquired, acquired);
        DistributedResilienceLogs.ProbeLeaseResult(_logger, key.Dependency, key.Operation, acquired);
        DistributedResilienceMeter.Shared.RecordOperation(
            "try-acquire-probe",
            acquired ? "success" : "not-acquired");
        return acquired ? new DistributedProbeLease(token) : null;
    }

    private static void AddStateCondition(
        ITransaction transaction,
        RedisKey stateKey,
        RedisValue currentValue)
    {
        if (currentValue.IsNull)
        {
            transaction.AddCondition(Condition.KeyNotExists(stateKey));
        }
        else
        {
            transaction.AddCondition(Condition.StringEqual(stateKey, currentValue));
        }
    }

    private static DistributedCircuitRecord? DeserializeState(RedisValue value)
    {
        if (value.IsNull)
        {
            return null;
        }

        return JsonSerializer.Deserialize<DistributedCircuitRecord>((string)value!, JsonOptions)
            ?? throw new InvalidOperationException("Redis contains an empty distributed circuit state record.");
    }

    private static RedisValue SerializeState(DistributedCircuitRecord record) =>
        JsonSerializer.Serialize(record, JsonOptions);

    private static DistributedCircuitState GetCircuitState(DistributedCircuitRecord? record)
    {
        if (record is null)
        {
            return DistributedCircuitState.Closed;
        }

        return record.OpenUntil > DateTimeOffset.UtcNow
            ? DistributedCircuitState.Open
            : DistributedCircuitState.HalfOpen;
    }

    private IDatabase GetDatabase() => _connectionMultiplexer.GetDatabase();

    private (RedisKey State, RedisKey Probe) CreateRedisKeys(DistributedCircuitKey key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key.Dependency);
        ArgumentException.ThrowIfNullOrWhiteSpace(key.Operation);

        var identity = $"{key.Dependency.Length}:{key.Dependency}{key.Operation}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
        var prefix = _options.KeyPrefix.TrimEnd(':');
        var hashTag = $"{{{hash}}}";

        return ($"{prefix}:{hashTag}:state", $"{prefix}:{hashTag}:probe");
    }

    private static DistributedResilienceOptions GetValidatedOptions(
        IOptions<DistributedResilienceOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Value.Validate();
        return options.Value;
    }

    private static long ToMilliseconds(TimeSpan duration)
    {
        var milliseconds = checked((long)Math.Ceiling(duration.TotalMilliseconds));
        if (milliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be at least one millisecond.");
        }

        return milliseconds;
    }

    private static void ValidateDuration(TimeSpan duration, string parameterName)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Duration must be greater than zero.");
        }

        _ = ToMilliseconds(duration);
    }

    private static void SetActivityTags(Activity? activity, DistributedCircuitKey key, string operation)
    {
        activity?.SetTag(DistributedResilienceActivityTagConstants.Dependency, key.Dependency);
        activity?.SetTag(DistributedResilienceActivityTagConstants.Operation, key.Operation);
        activity?.SetTag(DistributedResilienceActivityTagConstants.OperationType, operation);
    }

    private void RecordFailure(
        Activity? activity,
        Exception exception,
        DistributedCircuitKey key,
        string operation)
    {
        activity?.SetStatus(ActivityStatusCode.Error, "Distributed circuit store operation failed.");
        activity?.SetTag(DistributedResilienceActivityTagConstants.Outcome, "error");
        DistributedResilienceLogs.StoreOperationFailed(_logger, exception, key.Dependency, key.Operation, operation);
        DistributedResilienceMeter.Shared.RecordOperation(operation, "error");
    }

    private sealed record DistributedCircuitRecord(DateTimeOffset OpenUntil);
}
