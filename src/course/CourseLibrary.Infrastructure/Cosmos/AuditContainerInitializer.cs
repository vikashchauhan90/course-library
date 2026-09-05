using CourseLibrary.Infrastructure.Configuration.Cosmos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Infrastructure.Cosmos;

internal sealed class AuditContainerInitializer(
    CosmosClient client,
    IOptions<CosmosOptions> options) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var database = client.GetDatabase(options.Value.DatabaseName);

        await CreateContainerAsync(database, "courses", cancellationToken);
        await CreateContainerAsync(database, "authors", cancellationToken);
        await CreateContainerAsync(database, "discussions", cancellationToken);
        await CreateContainerAsync(database, "comments", cancellationToken);
        await CreateContainerAsync(database, "author-audit", cancellationToken);
        await CreateContainerAsync(database, "course-audit", cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static Task CreateContainerAsync(
        Database database,
        string containerName,
        CancellationToken cancellationToken)
    {
        return database.CreateContainerIfNotExistsAsync(
            new ContainerProperties(containerName, "/partitionKeyValue"),
            cancellationToken: cancellationToken);
    }
}
