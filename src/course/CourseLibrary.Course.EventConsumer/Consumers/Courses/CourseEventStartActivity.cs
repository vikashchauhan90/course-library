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
        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            [CourseEventConsumerActivityTagConstants.MessagingMessageId] = input.MessageId,
            [CourseEventConsumerActivityTagConstants.MessagingCorrelationId] =
                input.Metadata.Properties.TryGetValue("CorrelationId", out var correlationId)
                    ? correlationId
                    : null,
            [CourseEventConsumerActivityTagConstants.OrchestrationInstanceId] = input.InstanceId
        });

        try
        {
            using var activity = StartRemoteActivity(
                "activity.course-event",
                input.Metadata.TraceParent,
                input.Metadata.TraceState);

            if (activity is null)
            {
                logger.ActivityCreationFailed(
                    input.Event.EventId);
                return Task.FromResult(input.Metadata.TraceParent);
            }

            activity.SetTag(
                CourseEventConsumerActivityTagConstants.OrchestrationName,
                input.OrchestrationName);
            activity.SetTag(
                CourseEventConsumerActivityTagConstants.OrchestrationInstanceId,
                input.InstanceId);
            activity.SetTag(
                CourseEventConsumerActivityTagConstants.OrchestrationStartTime,
                input.StartTime.ToString("O"));
            activity.SetTag(
                CourseEventConsumerActivityTagConstants.OrchestrationIsReplaying,
                input.IsReplaying);
            activity.SetTag(
                CourseEventConsumerActivityTagConstants.ActivityName,
                nameof(CourseEventStartActivity));
            activity.SetTag(
                CourseEventConsumerActivityTagConstants.ActivityEventType,
                "CourseEvent");
            activity.SetTag(
                CourseEventConsumerActivityTagConstants.ActivityCourseId,
                input.Event.CourseId);
            activity.SetTag(
                CourseEventConsumerActivityTagConstants.ActivityEventId,
                input.Event.EventId);
            foreach (var item in input.Metadata.Baggage)
            {
                activity.AddBaggage(item.Key, item.Value);
            }

            activity.SetStatus(ActivityStatusCode.Ok);

            return Task.FromResult<string?>(activity.Id);
        }
        catch (Exception exception)
        {
            logger.CourseEventProcessingFailed(
                exception,
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
