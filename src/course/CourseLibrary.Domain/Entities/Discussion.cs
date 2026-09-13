using CourseLibrary.Domain.Abstractions;
using CourseLibrary.Domain.ValueObjects;

namespace CourseLibrary.Domain.Entities;

public sealed record Discussion : IEntity<DiscussionId>, IAuditableEntity
{
    public required DiscussionId Id { get; init; }
    public required CourseId CourseId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
}
