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
    DateTimeOffset OccurredAt) : ICommand<Unit>;
