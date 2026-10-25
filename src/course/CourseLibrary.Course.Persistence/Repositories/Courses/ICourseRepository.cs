using CourseLibrary.Course.Contracts.DTOs.Courses;
using CourseLibrary.Course.Contracts.DTOs;

namespace CourseLibrary.Course.Persistence.Repositories.Courses;

public interface ICourseRepository
{
    Task<CourseResponse?> GetByIdAsync(string courseId, CancellationToken cancellationToken = default);
    Task<PageResult<CourseResponse>> GetByAuthorAsync(
        string authorId,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default);
    Task<PageResult<CourseResponse>> SearchAsync(
        CourseSearchCriteria criteria,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default);
    Task UpsertAsync(CourseResponse course, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string courseId, string authorId, CancellationToken cancellationToken = default);
}
