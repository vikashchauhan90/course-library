using System.Collections.Immutable;
using System.Diagnostics;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.Events;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Platform.Messaging.Abstractions;
using CourseLibrary.Platform.Serialization.Abstractions;
using Microsoft.Extensions.Logging;
using CourseEntity = CourseLibrary.Course.Domain.Entities.Course;

namespace CourseLibrary.Course.Application.UseCases.Courses;

internal sealed class CourseEventPublisher(
    IEventPublisher eventPublisher,
    ISerializerFactory serializerFactory,
    ILogger<CourseEventPublisher> logger)
    : ICourseEventPublisher
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
                EventKind = eventKind.ToString(),
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

        logger.CourseEventPublished(eventKind, course.Id);
    }
}
