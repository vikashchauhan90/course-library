namespace CourseLibrary.Platform.Persistence.CosmosDB.Observability;

internal static class CosmosMetricConstants
{
    public const string OperationsInstrumentName = "course_library.cosmos.operations";
    public const string FailuresInstrumentName = "course_library.cosmos.operation.failures";
    public const string DurationInstrumentName = "course_library.cosmos.operation.duration";
    public const string RequestChargeInstrumentName = "course_library.cosmos.request_charge";
}
