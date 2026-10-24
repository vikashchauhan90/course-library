namespace CourseLibrary.Course.Application.Observability;

public static class CourseApplicationActivityTagConstants
{
    public const string UseCase = "app.use_case";
    public const string UseCaseSuccess = "app.use_case.success";
    public const string UseCaseCancelled = "app.use_case.cancelled";
    public const string DurationMilliseconds = "app.use_case.duration_ms";
    public const string ErrorType = "error.type";
}
