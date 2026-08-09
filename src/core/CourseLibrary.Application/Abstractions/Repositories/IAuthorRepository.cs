using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Application.Abstractions.Repositories;

public interface IAuthorRepository
{
    Task<Author?> GetByIdAsync(string authorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpsertAsync(Author author, CancellationToken cancellationToken = default);
    Task DeleteAsync(string authorId, CancellationToken cancellationToken = default);
}
