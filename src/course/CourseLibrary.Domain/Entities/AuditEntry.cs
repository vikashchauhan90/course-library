using CourseLibrary.Domain.Abstractions;
using System.Text.Json.Serialization;

namespace CourseLibrary.Domain.Entities;

public class AuditEntry
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required AuditAction Action { get; init; }
    public required string Name { get; init; }
    public  object? Value { get; init; }
    public string? ValueTypeName => Value?.GetType().FullName;
}
