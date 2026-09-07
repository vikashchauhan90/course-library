using CourseLibrary.Domain.Abstractions;
using System.Text.Json.Serialization;

namespace CourseLibrary.Domain.Entities;

[CosmosContainer("course-audit")]
public sealed record CourseAuditEntry : ICosmosPartitioned
{
    public required string Id { get; init; }
    public required string CourseId { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required AuditAction Action { get; init; }
    public string? EventId { get; init; }
    public IReadOnlyList<AuditEntry>? ChangedProperties { get; init; }
    public string? ActorId { get; init; }
    public string PartitionKeyValue => CourseId;
}
