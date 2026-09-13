using CourseLibrary.Models;
using Microsoft.Azure.Cosmos;

namespace CourseLibrary.Infrastructure.Cosmos;

public interface ICosmosRepository<TDocument, TKey>
    where TDocument : class
{
    Task<TDocument?> GetByIdAsync(
        TKey id,
        string partitionKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TDocument>> QueryAsync(
        QueryDefinition query,
        string? partitionKey = null,
        CancellationToken cancellationToken = default);

    Task<PageResult<TDocument>> QueryPageAsync(
        QueryDefinition query,
        string? partitionKey = null,
        string? continuationToken = null,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        TDocument item,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        TKey id,
        string partitionKey,
        CancellationToken cancellationToken = default);
}