namespace CourseLibrary.Course.Application.Observability;

public static class CourseApplicationMetricConstants
{
    public const string OperationsInstrumentName = "course_library.application.operations";
    public const string FailuresInstrumentName = "course_library.application.operation.failures";
    public const string DurationInstrumentName = "course_library.application.operation.duration";
}
