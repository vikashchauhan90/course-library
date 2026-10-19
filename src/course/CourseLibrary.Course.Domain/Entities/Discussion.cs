using CourseLibrary.Core.Domain.Abstractions.Auditing;
using CourseLibrary.Core.Domain.Abstractions.Entities;
using CourseLibrary.Course.Domain.ValueObjects;

namespace CourseLibrary.Course.Domain.Entities;

public sealed record Discussion : IEntity<DiscussionId>, IAuditableEntity
{
    public required DiscussionId Id { get; init; }
    public required CourseId CourseId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
