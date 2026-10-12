using CourseLibrary.Course.Contracts.Pagination;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;

namespace CourseLibrary.Course.Persistence.Abstractions.Repositories;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(CourseId courseId, CancellationToken cancellationToken = default);
    Task<PageResult<Course>> GetByAuthorAsync(
        AuthorId authorId,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default);
    Task<PageResult<Course>> SearchAsync(
        CourseSearchCriteria criteria,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default);
    Task UpsertAsync(Course course, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(CourseId courseId, AuthorId partitionKey, CancellationToken cancellationToken = default);
}
