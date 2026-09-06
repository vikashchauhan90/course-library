using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Consumers.Courses.CreateCourse;

internal sealed class CreateCourseOrchestrator
{
    [Function(nameof(CreateCourseOrchestrator))]
    public static async Task RunAsync([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger<CreateCourseOrchestrator>();
        using var activity = ActivitySources.EventConsumer.StartActivity("orchestration.create-course", ActivityKind.Internal);
        var courseEvent = context.GetInput<CourseCreatedEvent>();
        if (courseEvent is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Missing event input");
            logger.LogError("CreateCourse orchestration {InstanceId} received no event.", context.InstanceId);
            throw new InvalidOperationException("CourseCreatedEvent was not provided.");
        }
        activity?.SetTag("orchestration.instance_id", context.InstanceId);
        activity?.SetTag("orchestration.name", nameof(CreateCourseOrchestrator));
        await context.CallActivityAsync(nameof(CreateCourseAuditActivity), courseEvent);
        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
