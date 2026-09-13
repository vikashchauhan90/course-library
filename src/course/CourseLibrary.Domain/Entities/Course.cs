using CourseLibrary.Domain.Abstractions;
using CourseLibrary.Domain.ValueObjects;

namespace CourseLibrary.Domain.Entities;

public sealed record Course : IEntity<CourseId>, IAuditableEntity
{
    public required CourseId Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required AuthorId AuthorId { get; init; }
    public required string AuthorName { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public DateTimeOffset? RetiredAt { get; init; }
}
