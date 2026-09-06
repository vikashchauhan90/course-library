using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Consumers.Courses.DeleteCourse;

internal sealed class DeleteCourseOrchestrator
{
    [Function(nameof(DeleteCourseOrchestrator))]
    public static async Task RunAsync([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger<DeleteCourseOrchestrator>();
        using var activity = ActivitySources.EventConsumer.StartActivity("orchestration.delete-course", ActivityKind.Internal);
        var courseEvent = context.GetInput<CourseDeletedEvent>();
        if (courseEvent is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Missing event input");
            logger.LogError("DeleteCourse orchestration {InstanceId} received no event.", context.InstanceId);
            throw new InvalidOperationException("CourseDeletedEvent was not provided.");
        }
        activity?.SetTag("orchestration.instance_id", context.InstanceId);
        activity?.SetTag("orchestration.name", nameof(DeleteCourseOrchestrator));
        await context.CallActivityAsync(nameof(DeleteCourseAuditActivity), courseEvent);
        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
