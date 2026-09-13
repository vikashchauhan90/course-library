using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using CourseLibrary.EventConsumer.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Consumers.Courses;

internal sealed class CourseEventStartActivity(
    ILogger<CourseEventStartActivity> logger)
{
    [Function(nameof(CourseEventStartActivity))]
    public Task<string?> RunAsync(
        [ActivityTrigger] OrchestrationContext<CourseEventPayload> input)
    {
        var beforeActivityInstance = Activity.Current;
        try
        {
            // Clear the current activity context to avoid propagating the orchestration activity
            Activity.Current = null;
            var courseEvent = input.Event;
            using var activity = ActivitySources.StartActivity(
                "activity.course-event",
                ActivityKind.Internal,
                input.ParentTraceParent,
                input.ParentTraceState);

            if (activity is null)
            {
                logger.LogWarning(
                    "Unable to create tracing activity for CourseEvent {EventId}.",
                    courseEvent.EventId);

                return Task.FromResult<string?>(null);
            }

            activity.SetTag(
            "orchestration.name",
            input.OrchestrationName);

            activity.SetTag(
                "orchestration.instance_id",
                input.InstanceId);

            activity.SetTag(
                "orchestration.start_time",
                input.StartTime.ToString("O"));

            activity.SetTag(
                "orchestration.is_replaying",
                input.IsReplaying);

            activity.SetTag(
                "activity.name",
                nameof(CourseEventStartActivity));

            activity.SetTag(
                "activity.event_type",
                courseEvent.EventType);

            activity.SetTag(
                "activity.course_id",
                courseEvent.CourseId);

            activity.SetTag(
                "activity.event_id",
                courseEvent.EventId);

            activity.SetStatus(ActivityStatusCode.Ok);

            // Activity.Id is the W3C traceparent when
            // Activity.DefaultIdFormat == ActivityIdFormat.W3C.
            return Task.FromResult<string?>(activity.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error processing CourseEvent {EventId} in orchestration {InstanceId}.",
                input.Event.EventId,
                input.InstanceId);
            throw;
        }
        finally
        {
            // Restore the previous activity context
            Activity.Current = beforeActivityInstance;
        }
    }
}