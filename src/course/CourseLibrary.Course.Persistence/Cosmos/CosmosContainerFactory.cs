using CourseLibrary.Course.Persistence.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace CourseLibrary.Course.Persistence.Cosmos;

internal class CosmosContainerFactory : ICosmosContainerFactory
{
    private readonly string _databaseName;
    private readonly CosmosClient _client;

    private readonly ConcurrentDictionary<string, Container>
        _containers = new(StringComparer.Ordinal);
    public CosmosContainerFactory(
        CosmosClient client,
        IOptions<CosmosOptions> options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            options.Value.DatabaseName);

        _databaseName = options.Value.DatabaseName;
        _client = client;

    }
    public Container GetContainer(string containerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);
        SupportedContainers
            .ValidateContainer(containerName);

        return _containers.GetOrAdd(
           containerName,
           name =>
           _client.GetContainer(_databaseName, name));
    }
}
