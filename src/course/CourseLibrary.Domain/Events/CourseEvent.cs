using CourseLibrary.Domain.Abstractions;
using System.Text.Json.Serialization;

namespace CourseLibrary.Domain.Events;

public sealed class CourseEvent : AuditableDomainEvent
{
    public required string CourseId { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required CourseEventType EventType { get; init; }
}
 