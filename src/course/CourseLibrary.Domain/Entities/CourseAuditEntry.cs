using CourseLibrary.Domain.Abstractions;
using CourseLibrary.Domain.Events;
using System.Text.Json.Serialization;

namespace CourseLibrary.Domain.Entities;

public sealed record CourseAuditEntry : IEntity
{
    public required string Id { get; init; }
    public required string CourseId { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required CourseEventType Action { get; init; }
    public string? EventId { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<AuditEntry>? ChangedProperties { get; init; }
    public string? ActorId { get; init; }
}
