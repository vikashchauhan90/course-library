using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Application.Abstractions.Repositories;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(string courseId, string partitionKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetByAuthorAsync(string authorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> SearchAsync(string query, int pageSize, string? continuationToken, CancellationToken cancellationToken = default);
    Task UpsertAsync(Course course, CancellationToken cancellationToken = default);
    Task DeleteAsync(string courseId, string partitionKey, CancellationToken cancellationToken = default);
}
