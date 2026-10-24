namespace CourseLibrary.Course.Persistence.Entities;

internal interface IEntity
{
    public string? ConcurrencyStamp { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    string PartitionKey { get; }
}

internal interface IEntity<TId> : IEntity
    where TId : notnull
{
    TId Id { get; }
}
