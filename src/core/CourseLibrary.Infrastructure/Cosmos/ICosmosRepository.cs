using CourseLibrary.Domain.Abstractions;
using CourseLibrary.Domain.Models;
using Microsoft.Azure.Cosmos;

namespace CourseLibrary.Infrastructure.Cosmos;

public interface ICosmosRepository<TDocument>
    where TDocument : ICosmosPartitioned
{
    Task<TDocument?> GetByIdAsync(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TDocument>> QueryAsync(
        QueryDefinition query,
        string partitionKey,
        CancellationToken cancellationToken = default);

    Task<PageResult<TDocument>> QueryPageAsync(
        QueryDefinition query,
        string partitionKey,
        string? continuationToken = null,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        TDocument item,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default);
}