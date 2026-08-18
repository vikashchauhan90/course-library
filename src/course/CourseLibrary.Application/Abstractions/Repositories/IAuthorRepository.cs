using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Models;

namespace CourseLibrary.Application.Abstractions.Repositories;

public interface IAuthorRepository
{
    Task<Author?> GetByIdAsync(string authorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpsertAsync(Author author, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string authorId, CancellationToken cancellationToken = default);
    Task<PageResult<Author>> QueryPageAsync(int pageSize, string? pageToken, CancellationToken cancellationToken = default);
}
