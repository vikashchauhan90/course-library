using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Application.Abstractions.Repositories;

public interface ICourseAuditRepository
{
    Task AddAsync(CourseAuditEntry entry, CancellationToken cancellationToken = default);
}
