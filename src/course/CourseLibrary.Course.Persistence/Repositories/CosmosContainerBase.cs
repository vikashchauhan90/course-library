using CourseLibrary.Course.Persistence.Observability;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Course.Persistence.Repositories;

internal abstract class CosmosContainerBase
{
    private readonly Lazy<Container> _container;
    private readonly CosmosOptions _options;
    private readonly CosmosClient _client;
    protected static readonly CourseActivitySource ActivitySource = CourseActivitySource.Shared;
    protected static readonly CourseMeter Meter = CourseMeter.Shared;
   protected Container Container =>
       _container.Value;

    public CosmosContainerBase(
        CosmosClient client,
        IOptions<CosmosOptions> options,
        string containerName)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNullOrEmpty(containerName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            options.Value.DatabaseName);

        _options = options.Value;
        _client = client;

        _container = new Lazy<Container>(
            () => _client.GetContainer(
                _options.DatabaseName,
                containerName),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }
 
}