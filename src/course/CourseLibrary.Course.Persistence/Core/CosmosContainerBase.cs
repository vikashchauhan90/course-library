using CourseLibrary.Course.Persistence.Observability;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Course.Persistence.Core;

internal abstract class CosmosContainerBase
{
    private readonly CosmosOptions _options;
    private readonly CosmosClient _client;
    protected static readonly CourseActivitySource ActivitySource = CourseActivitySource.Shared;
    protected static readonly CourseMeter Meter = CourseMeter.Shared;
    public CosmosContainerBase(
        CosmosClient client,
        IOptions<CosmosOptions> options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            options.Value.DatabaseName);

        _options = options.Value;
        _client = client;

    }

    protected Container GetContainer(string containerName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(containerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            _options.DatabaseName);
        return _client.GetContainer(
            _options.DatabaseName,
            containerName);
    }

}