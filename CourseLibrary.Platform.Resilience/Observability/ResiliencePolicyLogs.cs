using Microsoft.Extensions.Logging;

namespace CourseLibrary.Platform.Resilience.Observability;

internal static partial class ResiliencePolicyLogs
{
    [LoggerMessage(
        5304,
        LogLevel.Debug,
        "Resilience policy {PolicyName} pipeline retrieved.",
        EventName = nameof(PipelineRetrieved))]
    public static partial void PipelineRetrieved(
        this ILogger logger,
        string policyName);

    [LoggerMessage(
        5305,
        LogLevel.Error,
        "Resilience policy {PolicyName} pipeline retrieval failed.",
        EventName = nameof(PipelineRetrievalFailed))]
    public static partial void PipelineRetrievalFailed(
        this ILogger logger,
        Exception exception,
        string policyName);

    [LoggerMessage(
        5301,
        LogLevel.Debug,
        "Resilience policy {PolicyName} execution completed.",
        EventName = nameof(ExecutionSucceeded))]
    public static partial void ExecutionSucceeded(
        this ILogger logger,
        string policyName);

    [LoggerMessage(
        5302,
        LogLevel.Debug,
        "Resilience policy {PolicyName} execution was cancelled.",
        EventName = nameof(ExecutionCancelled))]
    public static partial void ExecutionCancelled(
        this ILogger logger,
        string policyName);

    [LoggerMessage(
        5303,
        LogLevel.Error,
        "Resilience policy {PolicyName} execution failed.",
        EventName = nameof(ExecutionFailed))]
    public static partial void ExecutionFailed(
        this ILogger logger,
        Exception exception,
        string policyName);
}