using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Domain.Entities;

public sealed record Discussion : IEntity<string>, IAuditableEntity
{
    public required string Id { get; init; }
    public required string CourseId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
}
