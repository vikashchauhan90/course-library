using CourseLibrary.Core.Domain.Abstractions.Auditing;
using CourseLibrary.Core.Domain.Abstractions.Entities;
using CourseLibrary.Course.Domain.ValueObjects;

namespace CourseLibrary.Course.Domain.Entities;

public sealed record Course : IEntity<CourseId>, IAuditableEntity
{
    public required CourseId Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required AuthorId AuthorId { get; init; }
    public required string AuthorName { get; init; }
    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset? RetiredAt { get; init; }
}
