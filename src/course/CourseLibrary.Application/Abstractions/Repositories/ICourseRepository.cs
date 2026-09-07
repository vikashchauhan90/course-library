using CourseLibrary.Domain.Entities;
using CourseLibrary.Models;
using CourseLibrary.Models.Course;

namespace CourseLibrary.Application.Abstractions.Repositories;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(string courseId, CancellationToken cancellationToken = default);
    Task<PageResult<Course>> GetByAuthorAsync(
        string authorId,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default);
    Task<PageResult<Course>> SearchAsync(
        CourseSearchCriteria criteria,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default);
    Task UpsertAsync(Course course, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string courseId, string partitionKey, CancellationToken cancellationToken = default);
}
