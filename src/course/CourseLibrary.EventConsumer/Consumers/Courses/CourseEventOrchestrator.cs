using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Consumers.Courses;

internal sealed class CourseEventOrchestrator
{
    [Function(nameof(CourseEventOrchestrator))]
    public static async Task RunAsync([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger<CourseEventOrchestrator>();
        using var activity = ActivitySources.EventConsumer.StartActivity("orchestration.course-event", ActivityKind.Internal);
        var courseEvent = context.GetInput<CourseEvent>();
        if (courseEvent is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Missing event input");
            logger.LogError("CreateCourse orchestration {InstanceId} received no event.", context.InstanceId);
            throw new InvalidOperationException("CourseCreatedEvent was not provided.");
        }
        activity?.SetTag("orchestration.start_time", context.CurrentUtcDateTime.ToString("o"));
        activity?.SetTag("orchestration.instance_id", context.InstanceId);
        activity?.SetTag("orchestration.name", nameof(CourseEventOrchestrator));
        activity?.SetTag("orchestration.is_replaying", context.IsReplaying);
        activity?.SetTag("orchestration.event_type", courseEvent.EventType.ToString()); 
        await context.CallActivityAsync(nameof(CourseEventAuditActivity), courseEvent);
        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
