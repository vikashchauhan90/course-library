using CourseLibrary.Course.Contracts.DTOs.Courses;
using CourseLibrary.Course.Contracts.DTOs.Courses.Audits;
using CourseLibrary.Course.Contracts.ValueObjects;

namespace CourseLibrary.Course.Application.UseCases.Courses;

internal interface ICourseEventPublisher
{
    Task PublishAsync(
        CourseResponse course,
        string actorId,
        CourseEventType eventKind,
        DateTimeOffset occurredAt,
        IReadOnlyList<AuditEntry> changes,
        CancellationToken cancellationToken);
}
