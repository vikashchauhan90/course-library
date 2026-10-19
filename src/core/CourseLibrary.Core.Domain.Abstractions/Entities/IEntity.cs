namespace CourseLibrary.Core.Domain.Abstractions.Entities;

public interface IEntity
{
}

public interface IEntity<TId> : IEntity
    where TId : notnull
{
    TId Id { get; }
}
