using CourseLibrary.Course.EventConsumer.Core;
using CourseLibrary.Course.EventConsumer.Observability;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace CourseLibrary.Course.EventConsumer.Consumers.Courses;

internal sealed class CourseEventOrchestrator
{
    [Function(nameof(CourseEventOrchestrator))]
    public static async Task RunAsync(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger<CourseEventOrchestrator>();
        var input = context.GetInput<OrchestrationInput<CourseEventPayload>>();

        if (input?.Event is null)
        {
            logger.OrchestrationEventMissing(
                context.InstanceId);
            throw new InvalidOperationException("CourseEvent orchestration event was not provided.");
        }

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            [CourseEventConsumerActivityTagConstants.OrchestrationName] = nameof(CourseEventOrchestrator),
            [CourseEventConsumerActivityTagConstants.OrchestrationInstanceId] = context.InstanceId,
            [CourseEventConsumerActivityTagConstants.OrchestrationIsReplaying] = context.IsReplaying,
            [CourseEventConsumerActivityTagConstants.MessagingMessageId] = input.MessageId,
            [CourseEventConsumerActivityTagConstants.MessagingCorrelationId] = input.Metadata.Properties.TryGetValue(
                "CorrelationId",
                out var correlationId)
                    ? correlationId
                    : null,
            [CourseEventConsumerActivityTagConstants.MessagingEventId] = input.Metadata.Properties.TryGetValue(
                CourseLibrary.Platform.Messaging.Observability.ServiceBusTraceContext.EventId,
                out var eventId)
                    ? eventId
                    : input.Event.EventId
        });

        var orchestrationContext = new OrchestrationContext<CourseEventPayload>
        {
            Event = input.Event,
            MessageId = input.MessageId,
            InstanceId = context.InstanceId,
            OrchestrationName = nameof(CourseEventOrchestrator),
            StartTime = context.CurrentUtcDateTime,
            IsReplaying = context.IsReplaying,
            Metadata = input.Metadata
        };

        var parentActivityId = await context.CallActivityAsync<string?>(
            nameof(CourseEventStartActivity),
            orchestrationContext);

        await context.CallActivityAsync(
            nameof(CourseEventAuditActivity),
            new OrchestrationActivityInput<CourseEventPayload>
            {
                Event = input.Event,
                MessageId = input.MessageId,
                InstanceId = context.InstanceId,
                Metadata = input.Metadata,
                ParentActivityId = parentActivityId,
                ParentTraceState = input.Metadata.TraceState
            });
    }
}
