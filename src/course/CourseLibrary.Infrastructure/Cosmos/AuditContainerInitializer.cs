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

        await database.CreateContainerIfNotExistsAsync(
            new ContainerProperties("author-audit", "/authorId"),
            cancellationToken: cancellationToken);

        await database.CreateContainerIfNotExistsAsync(
            new ContainerProperties("course-audit", "/courseId"),
            cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
