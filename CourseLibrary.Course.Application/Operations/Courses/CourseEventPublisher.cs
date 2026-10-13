using System.Collections.Immutable;
using System.Diagnostics;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.Events;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Platform.Messaging.Abstractions;
using CourseLibrary.Platform.Serialization.Abstractions;
using CourseEntity = CourseLibrary.Course.Domain.Entities.Course;

namespace CourseLibrary.Course.Application.Operations.Courses;

internal sealed class CourseEventPublisher(IEventPublisher eventPublisher, ISerializerFactory serializerFactory)
{
    public async Task PublishAsync(
        CourseEntity course,
        string actorId,
        CourseEventType eventKind,
        DateTimeOffset occurredAt,
        IReadOnlyList<AuditEntry> changes,
        CancellationToken cancellationToken)
    {
        var eventId = Guid.NewGuid();
        var payload = serializerFactory
            .Create<CourseEvent>(SerializerType.Json)
            .Serialize(new CourseEvent
            {
                EventId = eventId.ToString(),
                CourseId = course.Id,
                ActorId = actorId,
                OccurredAt = occurredAt,
                EventKind = eventKind,
                ChangedProperties = changes
            });
        var activity = Activity.Current;

        await eventPublisher.PublishAsync(
            nameof(CourseEvent),
            new ServiceBusEventPayload
            {
                EventId = eventId,
                EventType = nameof(CourseEvent),
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
    }
}
