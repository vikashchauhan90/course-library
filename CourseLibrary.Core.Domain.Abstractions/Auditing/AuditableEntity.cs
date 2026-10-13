using CourseLibrary.Core.Domain.Abstractions.Entities;

namespace CourseLibrary.Core.Domain.Abstractions.Auditing;

public abstract class AuditableEntity<TId> :
    Entity<TId>,
    IAuditableEntity
    where TId : notnull
{
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset? UpdatedAt { get; protected set; }
    public DateTimeOffset? DeletedAt { get; protected set; }
}
