namespace CourseLibrary.Core.Domain.Abstractions.Auditing;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset? UpdatedAt { get; }
    DateTimeOffset? DeletedAt { get; }
}
