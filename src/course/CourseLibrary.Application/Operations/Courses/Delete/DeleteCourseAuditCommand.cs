using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Delete;

public sealed record DeleteCourseAuditCommand(
    string CourseId,
    string AuthorId,
    string AuthorName,
    string Title,
    string Description,
    string ActorId,
    DateTimeOffset OccurredAt) : ICommand<Unit>;
