using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseEntity = CourseLibrary.Course.Domain.Entities.Course;

namespace CourseLibrary.Course.Application.Operations.Courses;

internal interface ICourseEventPublisher
{
    Task PublishAsync(
        CourseEntity course,
        string actorId,
        CourseEventType eventKind,
        DateTimeOffset occurredAt,
        IReadOnlyList<AuditEntry> changes,
        CancellationToken cancellationToken);
}
