namespace CourseLibrary.Platform.Idempotency.Observability;

internal static class IdempotencyMeterConstants
{
    public const string MeterName = "CourseLibrary.Platform.Idempotency";
    public const string OperationsInstrumentName = "course_library.idempotency.operations";
    public const string DurationInstrumentName = "course_library.idempotency.duration";
    public const string OperationTag = "idempotency.operation";
    public const string OutcomeTag = "idempotency.outcome";
}