using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Application.Operations.Courses.Audit;
using CourseLibrary.Domain.Events;
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
     ISerializerFactory serializerFactory,
    ILogger<CourseEventAuditActivity> logger)
{
    private readonly ISerializer<CourseEvent> serializer = serializerFactory.Create<CourseEvent>(SerializerType.Json);

    [Function(nameof(CourseEventAuditActivity))]
    public async Task RunAsync(
        [ActivityTrigger] OrchestrationActivityInput input,
        CancellationToken cancellationToken)
    {
        var curseEvent = serializer.Deserialize(input.Event);
        var started = Stopwatch.GetTimestamp();
        using var activity = ActivitySources.EventConsumer.StartActivity(
            "activity.course-audit-event",
            ActivityKind.Internal,
            input.ParentActivityId);
        
        try
        {
            activity?.SetTag("activity.name", nameof(CourseEventAuditActivity));
            activity?.SetTag("activity.event_type", curseEvent.EventType.ToString());
            activity?.SetTag("activity.course_id", curseEvent.CourseId.ToString());
            activity?.SetTag("activity.event_id", curseEvent.EventId.ToString());
            await dispatcher.SendAsync<CourseAuditEventCommand, Unit>(new CourseAuditEventCommand(curseEvent), cancellationToken);
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
