using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Delete;

public sealed record DeleteCourseAuditCommand(
    string CourseId,
    string AuthorId,
    string AuthorName,
    string Title,
    string Description,
    string EventId,
    string ActorId,
    DateTimeOffset OccurredAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? RetiredAt,
    DateTimeOffset? DeletedAt,
    IReadOnlyList<string> ChangedProperties) : ICommand<Unit>;
