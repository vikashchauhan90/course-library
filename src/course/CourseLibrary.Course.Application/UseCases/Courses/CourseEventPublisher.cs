using System.Collections.Immutable;
using System.Diagnostics;
using CourseLibrary.Course.Contracts.ValueObjects;
using CourseLibrary.Course.Contracts.DTOs.Courses.Audits;
using CourseLibrary.Course.Contracts.DTOs.Courses;
using CourseLibrary.Platform.Messaging.Abstractions;
using CourseLibrary.Platform.Serialization.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.UseCases.Courses;

internal sealed class CourseEventPublisher(
    IEventPublisher eventPublisher,
    ISerializerFactory serializerFactory,
    ILogger<CourseEventPublisher> logger)
    : ICourseEventPublisher
{
    public async Task PublishAsync(
        CourseResponse course,
        string actorId,
        CourseEventType eventKind,
        DateTimeOffset occurredAt,
        IReadOnlyList<AuditEntry> changes,
        CancellationToken cancellationToken)
    {
        var eventId = Guid.NewGuid();
        var payload = serializerFactory
            .Create<CourseEventRequest>(SerializerType.Json)
            .Serialize(new CourseEventRequest
            {
                EventId = eventId.ToString(),
                CourseId = course.Id,
                ActorId = actorId,
                OccurredAt = occurredAt,
                EventKind = eventKind.ToString(),
                ChangedProperties = changes
            });
        var activity = Activity.Current;

        await eventPublisher.PublishAsync(
            "CourseEvent",
            new ServiceBusEventPayload
            {
                EventId = eventId,
                EventType = "CourseEvent",
                OccurredAt = occurredAt,
                Payload = new BinaryData(payload),
                Source = "CourseLibrary.Course.Application",
                Subject = course.Id.ToString(),
                Metadata = new EventMetadata
                {
                    TraceParent = activity?.Id,
                    TraceState = activity?.TraceStateString,
                    Properties = ImmutableDictionary<string, string>.Empty
                        .Add("actorId", actorId)
                        .Add("courseId", course.Id.ToString())
                }
            },
            cancellationToken);

        logger.CourseEventPublished(eventKind, course.Id);
    }
}
