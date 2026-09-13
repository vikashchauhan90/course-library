using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Application.Operations.Courses.Audit;

using CourseLibrary.EventConsumer.Configuration.Observability.Metrics;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using CourseLibrary.EventConsumer.Core;
using CourseLibrary.Infrastructure.Serializers;
using MediatorForge.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Consumers.Courses;

internal sealed class CourseEventAuditActivity(
    IDispatcher dispatcher,
    ILogger<CourseEventAuditActivity> logger)
{

    [Function(nameof(CourseEventAuditActivity))]
    public async Task RunAsync(
        [ActivityTrigger] OrchestrationActivityInput<CourseEventPayload> input,
        CancellationToken cancellationToken)
    {
        var curseEvent = input.Event.ToDomain();
        var started = Stopwatch.GetTimestamp();
        using var activity = ActivitySources.EventConsumer.StartActivity(
            "activity.course-audit-event",
            ActivityKind.Internal,
            input.ParentActivityId);
        
        try
        {
            var courseEvent = input.Event.ToDomain();
            activity?.SetTag("activity.name", nameof(CourseEventAuditActivity));
            activity?.SetTag("activity.event_type", courseEvent.EventType.ToString());
            activity?.SetTag("activity.course_id", courseEvent.CourseId.ToString());
            activity?.SetTag("activity.event_id", courseEvent.EventId);

            await dispatcher.SendAsync<CourseAuditEventCommand, Unit>(
                       new CourseAuditEventCommand(courseEvent),
                       cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            Meters.ActivitiesCompleted.Add(1, new TagList { { "activity", nameof(CourseEventAuditActivity) } });
            Meters.ActivityDuration.Record(Stopwatch.GetElapsedTime(started).TotalMilliseconds, new TagList { { "activity", nameof(CourseEventAuditActivity) } });
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            Meters.ActivitiesFailed.Add(1, new TagList { { "activity", nameof(CourseEventAuditActivity) } });
            logger.LogError(
                exception,
                "Failed to create course audit for CourseId {CourseId} and EventId {EventId}.",
                curseEvent.CourseId,
                curseEvent.EventId);
            throw;
        }
    }
}
