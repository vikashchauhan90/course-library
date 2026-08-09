using CourseLibrary.Domain.Abstractions;
using Microsoft.Azure.Cosmos;

namespace CourseLibrary.Infrastructure.Cosmos;

public sealed class CosmosRepository<T> : ICosmosRepository<T> where T : ICosmosPartitioned
{
    private readonly Container _container;

    public CosmosRepository(CosmosClient client, string databaseName, string containerName)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);

        _container = client.GetContainer(databaseName, containerName);
    }

    public async Task<T?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }
    }

    public async Task<IEnumerable<T>> QueryAsync(string query, string partitionKey, QueryRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        var queryDefinition = new QueryDefinition(query);
        var iterator = _container.GetItemQueryIterator<T>(queryDefinition, requestOptions: requestOptions ?? new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) });
        var results = new List<T>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response.Resource);
        }

        return results;
    }

    public async Task UpsertAsync(T item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        await _container.UpsertItemAsync(item, new PartitionKey(item.PartitionKeyValue), cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
    }
}
