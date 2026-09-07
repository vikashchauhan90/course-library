using MediatorForge.Abstractions;
using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Delete;

public sealed record DeleteCourseAuditCommand(
    string CourseId,
    string EventId,
    string ActorId,
    DateTimeOffset OccurredAt,
    IReadOnlyList<AuditEntry> ChangedProperties) : ICommand<Unit>;
