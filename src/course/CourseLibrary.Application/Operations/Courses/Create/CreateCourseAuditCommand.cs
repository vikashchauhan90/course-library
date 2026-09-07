using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Create;

public sealed record CreateCourseAuditCommand(
    string CourseId,
    string AuthorId,
    string Title,
    string Description,
    string AuthorName,
    string EventId,
    string ActorId,
    DateTimeOffset OccurredAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? RetiredAt,
    DateTimeOffset? DeletedAt,
    IReadOnlyList<string> ChangedProperties) : ICommand<Unit>;
