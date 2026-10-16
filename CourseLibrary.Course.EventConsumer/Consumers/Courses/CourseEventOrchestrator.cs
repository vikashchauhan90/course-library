using CourseLibrary.Course.EventConsumer.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

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
            logger.LogError(
                "CourseEvent orchestration {InstanceId} received no event.",
                context.InstanceId);
            throw new InvalidOperationException("CourseEvent orchestration event was not provided.");
        }

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["durable.orchestration.name"] = nameof(CourseEventOrchestrator),
            ["durable.instance_id"] = context.InstanceId,
            ["durable.is_replaying"] = context.IsReplaying
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
