using CourseLibrary.Core.Domain.Abstractions.Auditing;
using CourseLibrary.Core.Domain.Abstractions.Entities;

namespace CourseLibrary.Course.Domain.Entities;

public sealed record Discussion : IEntity<string>, IAuditableEntity, IPartitionedEntity
{
    public required string Id { get; init; }
    public required string CourseId { get; init; }
    public required string PartitionKey { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
