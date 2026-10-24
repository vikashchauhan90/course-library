using CourseLibrary.Course.Domain.Entities;

namespace CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;

public interface ICourseAuditRepository
{
    Task AddAsync(CourseAuditEntry entry, CancellationToken cancellationToken = default);
}
