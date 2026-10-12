using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;

namespace CourseLibrary.Course.Persistence.Abstractions.Repositories;

public interface ICourseAuditRepository
{
    Task AddAsync(CourseAuditEntry entry, CancellationToken cancellationToken = default);
}
