using CourseLibrary.Core.Results;
using CourseLibrary.Course.Contracts.DTOs;

namespace CourseLibrary.Course.Persistence.Repositories.Abstractions;

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
