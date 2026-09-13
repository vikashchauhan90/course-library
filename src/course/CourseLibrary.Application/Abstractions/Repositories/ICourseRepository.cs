using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.ValueObjects;
using CourseLibrary.Models;
using CourseLibrary.Models.Course;

namespace CourseLibrary.Application.Abstractions.Repositories;

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
