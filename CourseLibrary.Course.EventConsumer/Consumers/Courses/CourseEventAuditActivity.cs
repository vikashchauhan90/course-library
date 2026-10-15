using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.EventConsumer.Core;
using CourseLibrary.Course.EventConsumer.Observability;
using CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;
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
        using var activity = StartParentActivity(
            "activity.course-audit-event",
            input.ParentActivityId,
            input.ParentTraceState);

        activity?.SetTag("activity.name", nameof(CourseEventAuditActivity));
        activity?.SetTag("activity.event_type", courseEvent.EventType);
        activity?.SetTag("activity.course_id", courseEvent.CourseId.ToString());
        activity?.SetTag("activity.event_id", courseEvent.EventId);

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
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            CourseEventConsumerMeter.Shared.RecordActivity(
                nameof(CourseEventAuditActivity),
                "failure",
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
            logger.LogError(
                exception,
                "Failed to create course audit for CourseId {CourseId} and EventId {EventId}.",
                courseEvent.CourseId,
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
