namespace CourseLibrary.Course.Application.Observability;

public static class CourseApplicationActivityTagConstants
{
    public const string Operation = "app.operation";
    public const string OperationSuccess = "app.operation.success";
    public const string OperationCancelled = "app.operation.cancelled";
    public const string DurationMilliseconds = "app.operation.duration_ms";
    public const string ErrorType = "error.type";
}
