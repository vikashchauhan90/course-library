using CourseLibrary.Infrastructure.Configuration.Cosmos;
using CourseLibrary.Infrastructure.Cosmos.Configurations;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

internal sealed class CosmosContainerInitializer(
    CosmosClient client,
    IOptions<CosmosOptions> options,
    IEnumerable<ICosmosDocumentConfiguration<object>> configurations)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var database = client.GetDatabase(options.Value.DatabaseName);

        foreach (var configuration in configurations)
        {
            await CreateContainerAsync(
                database,
                configuration,
                cancellationToken);
        }
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
        => Task.CompletedTask;

    private static Task CreateContainerAsync(
        Database database,
        ICosmosDocumentConfiguration<object> configuration,
        CancellationToken cancellationToken)
    {
        return database.CreateContainerIfNotExistsAsync(
            new ContainerProperties(
                configuration.ContainerName,
                configuration.PartitionKeyPath),
            cancellationToken: cancellationToken);
    }
}