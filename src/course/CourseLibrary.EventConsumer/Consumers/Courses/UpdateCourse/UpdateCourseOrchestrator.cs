using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Consumers.Courses.UpdateCourse;

internal sealed class UpdateCourseOrchestrator
{
    [Function(nameof(UpdateCourseOrchestrator))]
    public static async Task RunAsync([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger<UpdateCourseOrchestrator>();
        using var activity = ActivitySources.EventConsumer.StartActivity("orchestration.update-course", ActivityKind.Internal);
        var courseEvent = context.GetInput<CourseUpdatedEvent>();
        if (courseEvent is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Missing event input");
            logger.LogError("UpdateCourse orchestration {InstanceId} received no event.", context.InstanceId);
            throw new InvalidOperationException("CourseUpdatedEvent was not provided.");
        }
        activity?.SetTag("orchestration.instance_id", context.InstanceId);
        activity?.SetTag("orchestration.name", nameof(UpdateCourseOrchestrator));
        await context.CallActivityAsync(nameof(UpdateCourseAuditActivity), courseEvent);
        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
