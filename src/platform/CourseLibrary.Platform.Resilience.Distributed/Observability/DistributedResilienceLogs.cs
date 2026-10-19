using Microsoft.Extensions.Logging;

namespace CourseLibrary.Platform.Resilience.Distributed.Observability;

internal static partial class DistributedResilienceLogs
{
    [LoggerMessage(
        5601,
        LogLevel.Debug,
        "Distributed circuit state for {Dependency}/{Operation} is {State}.",
        EventName = nameof(CircuitStateRead))]
    public static partial void CircuitStateRead(
        this ILogger logger,
        string dependency,
        string operation,
        string state);

    [LoggerMessage(
        5602,
        LogLevel.Warning,
        "Distributed circuit opened for {Dependency}/{Operation} for {Duration}.",
        EventName = nameof(CircuitOpened))]
    public static partial void CircuitOpened(
        this ILogger logger,
        string dependency,
        string operation,
        TimeSpan duration);

    [LoggerMessage(
        5603,
        LogLevel.Debug,
        "Distributed probe lease for {Dependency}/{Operation} acquired: {Acquired}.",
        EventName = nameof(ProbeLeaseResult))]
    public static partial void ProbeLeaseResult(
        this ILogger logger,
        string dependency,
        string operation,
        bool acquired);

    [LoggerMessage(
        5604,
        LogLevel.Information,
        "Distributed probe for {Dependency}/{Operation} completed. Succeeded: {Succeeded}, LeaseCompleted: {LeaseCompleted}.",
        EventName = nameof(ProbeCompleted))]
    public static partial void ProbeCompleted(
        this ILogger logger,
        string dependency,
        string operation,
        bool succeeded,
        bool leaseCompleted);

    [LoggerMessage(
        5605,
        LogLevel.Error,
        "Distributed circuit store operation {StoreOperation} failed for {Dependency}/{Operation}.",
        EventName = nameof(StoreOperationFailed))]
    public static partial void StoreOperationFailed(
        this ILogger logger,
        Exception exception,
        string dependency,
        string operation,
        string storeOperation);

    [LoggerMessage(
        5606,
        LogLevel.Debug,
        "Distributed resilience execution succeeded for {Dependency}/{Operation}.",
        EventName = nameof(PipelineExecutionSucceeded))]
    public static partial void PipelineExecutionSucceeded(
        this ILogger logger,
        string dependency,
        string operation);

    [LoggerMessage(
        5607,
        LogLevel.Debug,
        "Distributed resilience execution for {Dependency}/{Operation} was cancelled.",
        EventName = nameof(PipelineExecutionCancelled))]
    public static partial void PipelineExecutionCancelled(
        this ILogger logger,
        string dependency,
        string operation);

    [LoggerMessage(
        5608,
        LogLevel.Error,
        "Distributed resilience execution failed for {Dependency}/{Operation}.",
        EventName = nameof(PipelineExecutionFailed))]
    public static partial void PipelineExecutionFailed(
        this ILogger logger,
        Exception exception,
        string dependency,
        string operation);

    [LoggerMessage(
        5609,
        LogLevel.Warning,
        "Distributed circuit blocked a call to {Dependency}/{Operation} until {OpenUntil}.",
        EventName = nameof(CircuitCallBlocked))]
    public static partial void CircuitCallBlocked(
        this ILogger logger,
        string dependency,
        string operation,
        DateTimeOffset? openUntil);

    [LoggerMessage(
        5610,
        LogLevel.Debug,
        "Retry attempt {AttemptNumber} for {Dependency}/{Operation} scheduled after {Delay}.",
        EventName = nameof(RetryAttempted))]
    public static partial void RetryAttempted(
        this ILogger logger,
        string dependency,
        string operation,
        int attemptNumber,
        TimeSpan delay);
}
