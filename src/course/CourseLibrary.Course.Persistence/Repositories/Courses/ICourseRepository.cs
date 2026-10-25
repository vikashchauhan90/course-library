using CourseLibrary.Core.Results;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Contracts.DTOs.Courses;

namespace CourseLibrary.Course.Persistence.Repositories.Courses;

public interface ICourseRepository
{
    Task<Entities.Course?> GetByIdAsync(string courseId, CancellationToken cancellationToken = default);
    Task<PageResult<Entities.Course>> GetByAuthorAsync(
        string authorId,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default);
    Task<PageResult<Entities.Course>> SearchAsync(
        CourseSearchCriteria criteria,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default);
    Task UpsertAsync(Entities.Course course, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string courseId, string authorId, CancellationToken cancellationToken = default);
}
