using System.Text.Json.Serialization;

namespace CourseLibrary.Domain.Abstractions;

public class AuditEntry
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required AuditAction Action { get; init; }
    public required string Name { get; init; }
    public  object? Value { get; init; }
    public string? ValueTypeName => Value?.GetType().Name;
}
