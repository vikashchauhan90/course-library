using MediatorForge.Abstractions;
using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Update;

public sealed record UpdateCourseAuditCommand(
    string CourseId,
    string EventId,
    string ActorId,
    DateTimeOffset OccurredAt,
    IReadOnlyList<AuditEntry> ChangedProperties) : ICommand<Unit>;
