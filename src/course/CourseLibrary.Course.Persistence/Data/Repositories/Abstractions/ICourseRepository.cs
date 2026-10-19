using CourseLibrary.Core.Results;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Domain.ValueObjects;

namespace CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;

public interface ICourseRepository
{
    Task<Domain.Entities.Course?> GetByIdAsync(CourseId courseId, CancellationToken cancellationToken = default);
    Task<PageResult<Domain.Entities.Course>> GetByAuthorAsync(
        AuthorId authorId,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default);
    Task<PageResult<Domain.Entities.Course>> SearchAsync(
        CourseSearchCriteria criteria,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default);
    Task UpsertAsync(Domain.Entities.Course course, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(CourseId courseId, AuthorId partitionKey, CancellationToken cancellationToken = default);
}
