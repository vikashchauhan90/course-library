namespace CourseLibrary.Course.Persistence.Entities;

internal class AuditEntry
{
    public required string Action { get; init; }
    public required string Name { get; init; }
    public  object? Value { get; init; }
    public string? ValueTypeName { get; init; }
}
