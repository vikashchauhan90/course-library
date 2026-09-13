
namespace CourseLibrary.Domain.Abstractions;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset? UpdatedAt { get; }
    DateTimeOffset? DeletedAt { get; }
}
