using CourseLibrary.Core.Results;
using Microsoft.Azure.Cosmos;

namespace CourseLibrary.Platform.Persistence.CosmosDB.Repositories;

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
        string partitionKey,
        string? continuationToken = null,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TDocument>> GetManyAsync(
        IReadOnlyList<(TKey Id, string PartitionKey)> items,
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        TDocument item,
        CancellationToken cancellationToken = default);

    Task ReplaceAsync(
        TKey id,
        TDocument item,
        string partitionKey,
        CancellationToken cancellationToken = default);

    Task ReplaceAsync(
        TKey id,
        TDocument item,
        string partitionKey,
        string etag,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        TDocument item,
        CancellationToken cancellationToken = default);

    Task PatchAsync(
        TKey id,
        string partitionKey,
        IReadOnlyList<PatchOperation> operations,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        TKey id,
        string partitionKey,
        CancellationToken cancellationToken = default);

    Task<TDocument?> FirstOrDefaultAsync(
        QueryDefinition query,
        string? partitionKey = null,
        CancellationToken cancellationToken = default);

    Task<TDocument?> SingleOrDefaultAsync(
        QueryDefinition query,
        string? partitionKey = null,
        CancellationToken cancellationToken = default);

    Task ExecuteBatchAsync(
        string partitionKey,
        Action<TransactionalBatch> configure,
        CancellationToken cancellationToken = default);
}