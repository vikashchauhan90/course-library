namespace CourseLibrary.Core.Domain.Abstractions.Entities;

public abstract class Entity<TId> : IEntity<TId>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;
}
