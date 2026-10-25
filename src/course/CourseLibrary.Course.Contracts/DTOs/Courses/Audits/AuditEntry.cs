namespace CourseLibrary.Course.Contracts.DTOs.Courses.Audits;

public  record AuditEntry
{
    public required string Action { get; init; }
    public required string Name { get; init; }
    public  object? Value { get; init; }
    public string? ValueTypeName { get; init; }
}
