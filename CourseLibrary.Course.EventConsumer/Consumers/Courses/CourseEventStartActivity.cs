using CourseLibrary.Course.EventConsumer.Core;
using CourseLibrary.Course.EventConsumer.Observability;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Course.EventConsumer.Consumers.Courses;

internal sealed class CourseEventStartActivity(
    ILogger<CourseEventStartActivity> logger)
{
    [Function(nameof(CourseEventStartActivity))]
    public Task<string?> RunAsync(
        [ActivityTrigger] OrchestrationContext<CourseEventPayload> input)
    {
        var previousActivity = Activity.Current;
        Activity.Current = null;

        try
        {
            using var activity = StartRemoteActivity(
                "activity.course-event",
                input.Metadata.TraceParent,
                input.Metadata.TraceState);

            if (activity is null)
            {
                logger.LogWarning(
                    "Unable to create tracing activity for CourseEvent {EventId}.",
                    input.Event.EventId);
                return Task.FromResult(input.Metadata.TraceParent);
            }

            activity.SetTag("orchestration.name", input.OrchestrationName);
            activity.SetTag("orchestration.instance_id", input.InstanceId);
            activity.SetTag("orchestration.start_time", input.StartTime.ToString("O"));
            activity.SetTag("orchestration.is_replaying", input.IsReplaying);
            activity.SetTag("activity.name", nameof(CourseEventStartActivity));
            activity.SetTag("activity.event_type", nameof(CourseLibrary.Course.Domain.Events.CourseEvent));
            activity.SetTag("activity.course_id", input.Event.CourseId);
            activity.SetTag("activity.event_id", input.Event.EventId);
            activity.SetStatus(ActivityStatusCode.Ok);

            return Task.FromResult<string?>(activity.Id);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Error processing CourseEvent {EventId} in orchestration {InstanceId}.",
                input.Event.EventId,
                input.InstanceId);
            throw;
        }
        finally
        {
            Activity.Current = previousActivity;
        }
    }

    private static Activity? StartRemoteActivity(
        string name,
        string? traceParent,
        string? traceState)
    {
        if (!ActivityContext.TryParse(
                traceParent,
                traceState,
                isRemote: true,
                out var parentContext))
        {
            return CourseEventConsumerActivitySource.Shared.StartActivity(
                name,
                ActivityKind.Internal);
        }

        return CourseEventConsumerActivitySource.Shared.StartActivity(
            name,
            ActivityKind.Internal,
            parentContext);
    }
}
