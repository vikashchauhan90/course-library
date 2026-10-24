using CourseLibrary.Core.Results;
using Microsoft.Azure.Cosmos;

namespace CourseLibrary.Platform.Persistence.CosmosDB.Repositories;

public interface ICosmosRepository<TDocument, TKey>
    where TDocument : class
{
    Task<TDocument?> GetByIdAsync(
        TKey id,
        IReadOnlyList<object?> partitionKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TDocument>> QueryAsync(
        QueryDefinition query,
        IReadOnlyList<object?>? partitionKey = null,
        CancellationToken cancellationToken = default);

    Task<PageResult<TDocument>> QueryPageAsync(
        QueryDefinition query,
        IReadOnlyList<object?> partitionKey,
        string? continuationToken = null,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TDocument>> GetManyAsync(
        IReadOnlyList<(TKey Id, IReadOnlyList<object?> PartitionKey)> items,
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        TDocument item,
        CancellationToken cancellationToken = default);

    Task ReplaceAsync(
        TKey id,
        TDocument item,
        IReadOnlyList<object?> partitionKey,
        CancellationToken cancellationToken = default);

    Task ReplaceAsync(
        TKey id,
        TDocument item,
        IReadOnlyList<object?> partitionKey,
        string etag,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        TDocument item,
        CancellationToken cancellationToken = default);

    Task PatchAsync(
        TKey id,
        IReadOnlyList<object?> partitionKey,
        IReadOnlyList<PatchOperation> operations,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        TKey id,
        IReadOnlyList<object?> partitionKey,
        CancellationToken cancellationToken = default);

    Task<TDocument?> FirstOrDefaultAsync(
        QueryDefinition query,
        IReadOnlyList<object?>? partitionKey = null,
        CancellationToken cancellationToken = default);

    Task<TDocument?> SingleOrDefaultAsync(
        QueryDefinition query,
        IReadOnlyList<object?>? partitionKey = null,
        CancellationToken cancellationToken = default);

    Task ExecuteBatchAsync(
        IReadOnlyList<object?> partitionKey,
        Action<TransactionalBatch> configure,
        CancellationToken cancellationToken = default);
}