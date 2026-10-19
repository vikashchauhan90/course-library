namespace CourseLibrary.Core.Domain.Abstractions.Auditing;

public interface IAuditableEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
