using CourseLibrary.Course.Contracts.DTOs.Courses.Audits;

namespace CourseLibrary.Course.Persistence.Repositories.Courses.Audits;

public interface ICourseAuditRepository
{
    Task<CourseAuditResponse> AddAsync(
        CourseAuditRequest request,
        CancellationToken cancellationToken = default);
}
