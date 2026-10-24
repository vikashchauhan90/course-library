using CourseLibrary.Course.Persistence.Entities;

namespace CourseLibrary.Course.Persistence.Repositories.Abstractions;

public interface ICourseAuditRepository
{
    Task AddAsync(CourseAuditEntry entry, CancellationToken cancellationToken = default);
}
