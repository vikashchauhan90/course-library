
namespace CourseLibrary.Domain.Abstractions;

public class AuditEntry
{
    public required Guid Action { get; init; }
    public required string Name { get; init; }
    public  object? Value { get; init; }
    public string? ValueTypeName => Value?.GetType().Name;
}
