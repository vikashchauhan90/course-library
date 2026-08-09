using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Infrastructure.Cosmos;

public sealed class CosmosAuthorRepository : IAuthorRepository
{
    private readonly ICosmosRepository<Author> _repository;

    public CosmosAuthorRepository(ICosmosRepository<Author> repository)
    {
        _repository = repository;
    }

    public Task<Author?> GetByIdAsync(string authorId, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(authorId, authorId, cancellationToken);

    public Task<IReadOnlyList<Author>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = new Microsoft.Azure.Cosmos.QueryDefinition(
            "SELECT * FROM c ORDER BY c.updatedAt DESC");

        return _repository.QueryAsync(query, cancellationToken: cancellationToken);
    }

    public Task UpsertAsync(Author author, CancellationToken cancellationToken = default)
        => _repository.UpsertAsync(author, cancellationToken);

    public Task DeleteAsync(string authorId, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(authorId, authorId, cancellationToken);
}
