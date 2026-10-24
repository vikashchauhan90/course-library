using CourseLibrary.Course.Contracts.ValueObjects;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Persistence.Entities;
using CourseEntity = CourseLibrary.Course.Persistence.Entities.Course;

namespace CourseLibrary.Course.Application.UseCases.Courses;

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
