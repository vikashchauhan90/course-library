using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Update;

public sealed record UpdateCourseAuditCommand(
    string CourseId,
    string AuthorId,
    string Title,
    string Description,
    string ActorId,
    DateTimeOffset OccurredAt) : ICommand<Unit>;
