namespace CourseLibrary.Course.EventConsumer.Observability;

internal static class CourseEventConsumerMeterConstants
{
    public const string Messages = "course.event_consumer.messages";
    public const string Failures = "course.event_consumer.failures";
    public const string Duplicates = "course.event_consumer.duplicates";
    public const string Orchestrations = "course.event_consumer.orchestrations";
    public const string Activities = "course.event_consumer.activities";
    public const string FunctionExecutions = "course.event_consumer.function.executions";
    public const string ActiveFunctions = "course.event_consumer.function.active";
    public const string MessageDuration = "course.event_consumer.message.duration";
    public const string FunctionDuration = "course.event_consumer.function.duration";
    public const string ActivityDuration = "course.event_consumer.activity.duration";

    public const string Outcome = "outcome";
    public const string Operation = "operation";
    public const string ErrorType = "error.type";
    public const string Status = "status";
    public const string OrchestrationName = "orchestration.name";
    public const string ActivityName = "activity.name";
    public const string FunctionName = "function.name";
}
