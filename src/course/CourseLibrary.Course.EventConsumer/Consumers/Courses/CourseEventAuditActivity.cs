using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.EventConsumer.Core;
using CourseLibrary.Course.EventConsumer.Observability;
using CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;
using CourseLibrary.Platform.Observability.Traces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Course.EventConsumer.Consumers.Courses;

internal sealed class CourseEventAuditActivity(
    ICourseAuditRepository auditRepository,
    ILogger<CourseEventAuditActivity> logger)
{
    [Function(nameof(CourseEventAuditActivity))]
    public async Task RunAsync(
        [ActivityTrigger] OrchestrationActivityInput<CourseEventPayload> input,
        CancellationToken cancellationToken)
    {
        var courseEvent = input.Event.ToDomain();
        var startedAt = Stopwatch.GetTimestamp();
        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            [CourseEventConsumerActivityTagConstants.MessagingMessageId] = input.MessageId,
            [CourseEventConsumerActivityTagConstants.MessagingCorrelationId] =
                input.Metadata.Properties.TryGetValue("CorrelationId", out var correlationId)
                    ? correlationId
                    : null,
            [CourseEventConsumerActivityTagConstants.OrchestrationInstanceId] = input.InstanceId,
            [CourseEventConsumerActivityTagConstants.MessagingEventId] = courseEvent.EventId
        });

        using var activity = StartParentActivity(
            "activity.course-audit-event",
            input.ParentActivityId,
            input.ParentTraceState);

        activity?.SetTag(
            CourseEventConsumerActivityTagConstants.ActivityName,
            nameof(CourseEventAuditActivity));
        activity?.SetTag(
            CourseEventConsumerActivityTagConstants.ActivityEventType,
            courseEvent.EventType);
        activity?.SetTag(
            CourseEventConsumerActivityTagConstants.ActivityCourseId,
            courseEvent.CourseId.ToString());
        activity?.SetTag(
            CourseEventConsumerActivityTagConstants.ActivityEventId,
            courseEvent.EventId);
        foreach (var item in input.Metadata.Baggage)
        {
            activity?.AddBaggage(item.Key, item.Value);
        }

        try
        {
            await auditRepository.AddAsync(
                new CourseAuditEntry
                {
                    Id = courseEvent.EventId,
                    CourseId = courseEvent.CourseId,
                    Action = courseEvent.EventKind,
                    EventId = courseEvent.EventId,
                    OccurredAt = courseEvent.OccurredAt,
                    ChangedProperties = courseEvent.ChangedProperties,
                    ActorId = courseEvent.ActorId,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            CourseEventConsumerMeter.Shared.RecordActivity(
                nameof(CourseEventAuditActivity),
                "success",
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
        }
        catch (Exception exception)
        {
            ActivityEnrichmentHelper.RecordException(activity, exception);
            CourseEventConsumerMeter.Shared.RecordActivity(
                nameof(CourseEventAuditActivity),
                "failure",
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
            logger.CourseAuditCreationFailed(
                exception,
                courseEvent.CourseId.ToString(),
                courseEvent.EventId);
            throw;
        }
    }

    private static Activity? StartParentActivity(
        string name,
        string? parentActivityId,
        string? parentTraceState)
    {
        if (string.IsNullOrWhiteSpace(parentActivityId))
        {
            return CourseEventConsumerActivitySource.Shared.StartActivity(
                name,
                ActivityKind.Internal);
        }

        if (ActivityContext.TryParse(
                parentActivityId,
                parentTraceState,
                isRemote: true,
                out var parentContext))
        {
            return CourseEventConsumerActivitySource.Shared.StartActivity(
                name,
                ActivityKind.Internal,
                parentContext);
        }

        return CourseEventConsumerActivitySource.Shared.StartActivity(
            name,
            ActivityKind.Internal);
    }
}
