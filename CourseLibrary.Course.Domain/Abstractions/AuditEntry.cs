
using CourseLibrary.Course.Domain.ValueObjects;

namespace CourseLibrary.Course.Domain.Abstractions;

public class AuditEntry
{
    public required AuditAction Action { get; init; }
    public required string Name { get; init; }
    public  object? Value { get; init; }
    public string? ValueTypeName { get; init; }
}
