using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Contracts.Pagination;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseEntity = CourseLibrary.Course.Domain.Entities.Course;

namespace CourseLibrary.Course.Persistence.Abstractions.Repositories;

public interface ICourseRepository
{
    Task<CourseEntity?> GetByIdAsync(CourseId courseId, CancellationToken cancellationToken = default);
    Task<PageResult<CourseEntity>> GetByAuthorAsync(
        AuthorId authorId,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default);
    Task<PageResult<CourseEntity>> SearchAsync(
        CourseSearchCriteria criteria,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default);
    Task UpsertAsync(CourseEntity course, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(CourseId courseId, AuthorId partitionKey, CancellationToken cancellationToken = default);
}
