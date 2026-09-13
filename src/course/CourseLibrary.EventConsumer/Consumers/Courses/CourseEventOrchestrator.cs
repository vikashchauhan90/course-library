using CourseLibrary.EventConsumer.Core;
using CourseLibrary.Infrastructure.Serializers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.EventConsumer.Consumers.Courses;

internal sealed class CourseEventOrchestrator
{
    [Function(nameof(CourseEventOrchestrator))]
    public static async Task RunAsync(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger =
            context.CreateReplaySafeLogger<CourseEventOrchestrator>();
        using var scope = logger.BeginScope(
            new Dictionary<string, object?>
            {
                ["durable.orchestration.name"] = nameof(CourseEventOrchestrator),
                ["durable.instance_id"] = context.InstanceId,
                ["durable.is_replaying"] = context.IsReplaying
            });

        var orchestrationInput =
            context.GetInput<OrchestrationInput<CourseEventPayload>>();

        if (orchestrationInput?.Event is null)
        {
            logger.LogError(
                "CreateCourse orchestration {InstanceId} received no event.",
                context.InstanceId);

            throw new InvalidOperationException(
                "CourseCreatedEvent was not provided.");
        }

        var orchestrationContext =
            new OrchestrationContext<CourseEventPayload>
            {
                Event = orchestrationInput.Event,
                MessageId = orchestrationInput.MessageId,
                InstanceId = context.InstanceId,
                OrchestrationName = nameof(CourseEventOrchestrator),
                StartTime = context.CurrentUtcDateTime,
                IsReplaying = context.IsReplaying,
                ParentTraceParent = orchestrationInput.ParentTraceParent,
                ParentTraceState = orchestrationInput.ParentTraceState
            };

        var parentActivityId =
            await context.CallActivityAsync<string?>(
                nameof(CourseEventStartActivity),
                orchestrationContext);

        var orchestrationActivityInput = new OrchestrationActivityInput<CourseEventPayload>
        {
            Event = orchestrationInput.Event,
            MessageId = orchestrationInput.MessageId,
            ParentActivityId = parentActivityId
        };

        await context.CallActivityAsync(
            nameof(CourseEventAuditActivity),
            orchestrationActivityInput);
    }
}