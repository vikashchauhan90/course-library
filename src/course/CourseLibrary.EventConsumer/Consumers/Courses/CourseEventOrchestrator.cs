using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Core;
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

        var orchestrationInput =
            context.GetInput<OrchestrationInput<CourseEvent>>();

        if (orchestrationInput?.Event is null)
        {
            logger.LogError(
                "CreateCourse orchestration {InstanceId} received no event.",
                context.InstanceId);

            throw new InvalidOperationException(
                "CourseCreatedEvent was not provided.");
        }

        var orchestrationContext =
            new OrchestrationContext<CourseEvent>
            {
                Event = orchestrationInput.Event,
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

        var orchestrationActivityInput = new OrchestrationActivityInput<CourseEvent>
        {
            Event = orchestrationInput.Event,
            ParentActivityId = parentActivityId
        };

        await context.CallActivityAsync(
            nameof(CourseEventAuditActivity),
            orchestrationActivityInput);
    }
}