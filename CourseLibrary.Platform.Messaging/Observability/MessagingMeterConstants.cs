namespace CourseLibrary.Platform.Messaging.Observability;

internal static class MessagingMeterConstants
{
    public const string MeterName = MessagingTelemetry.MeterName;
    public const string OperationsInstrumentName = "course_library.messaging.operations";
    public const string DurationInstrumentName = "course_library.messaging.duration";
    public const string MessageSizeInstrumentName = "course_library.messaging.message.size";
    public const string OperationTag = "messaging.operation";
    public const string OutcomeTag = "messaging.outcome";
}
