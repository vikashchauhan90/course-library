namespace CourseLibrary.Platform.Resilience.Observability;

internal static class ResilienceActivityTagConstants
{
    public const string PolicyName = "resilience.policy.name";
    public const string OperationType = "resilience.operation.type";
    public const string Operation = "resilience.operation";
    public const string Outcome = "resilience.outcome";

    public const string ExecuteOperation = "execute";
    public const string GetPipelineOperation = "get-pipeline";
    public const string SuccessOutcome = "success";
    public const string CancelledOutcome = "cancelled";
    public const string ErrorOutcome = "error";
}