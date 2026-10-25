namespace CourseLibrary.Idp.Domain.Abstractions;

public interface IEntity<TId>
{
    TId Id { get; }
}

public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
}

public interface IConcurrency
{
    string? ConcurrencyStamp { get; set; }
}
