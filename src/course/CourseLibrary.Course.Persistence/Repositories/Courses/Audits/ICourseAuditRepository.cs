using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Contracts.DTOs.Courses.Audits;

namespace CourseLibrary.Course.Persistence.Repositories.Courses.Audits;

public interface ICourseAuditRepository
{
    Task<CourseAuditResponse> AddAsync(
        CourseAuditRequest request,
        CancellationToken cancellationToken = default);

    Task<CourseAuditResponse?> GetByIdAsync(
        string courseId,
        string id,
        CancellationToken cancellationToken = default);

    Task<PageResult<CourseAuditResponse>> GetPageAsync(
        string courseId,
        string? continuationToken = null,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}
