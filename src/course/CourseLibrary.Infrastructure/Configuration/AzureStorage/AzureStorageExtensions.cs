using Azure.Core;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Infrastructure.Messaging.AzureTable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Infrastructure.Configuration.AzureStorage;

public static class AzureStorageExtensions
{
    public static IServiceCollection AddCourseLibraryAzureStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<AzureStorageOptions>()
            .Bind(configuration.GetSection(AzureStorageOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "AzureStorage:ConnectionString must be configured.")
            .ValidateOnStart();

        services.AddSingleton<BlobServiceClient>(sp =>
        {
            var options = sp
                .GetRequiredService<IOptions<AzureStorageOptions>>()
                .Value;

            var clientOptions = new BlobClientOptions
            {
                Retry =
                {
                    Mode = RetryMode.Exponential,
                    MaxRetries = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    MaxDelay = TimeSpan.FromSeconds(5),
                    NetworkTimeout = TimeSpan.FromSeconds(10)
                }
            };

            return new BlobServiceClient(options.ConnectionString, clientOptions);
        });


        services.AddSingleton<TableServiceClient>(sp =>
        {
            var options = sp
                .GetRequiredService<IOptions<AzureStorageOptions>>()
                .Value;

            var clientOptions = new TableClientOptions
            {
                Retry =
                {
                    Mode = RetryMode.Exponential,
                    MaxRetries = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    MaxDelay = TimeSpan.FromSeconds(5),
                    NetworkTimeout = TimeSpan.FromSeconds(10)
                }
            };

            return new TableServiceClient(options.ConnectionString, clientOptions);
        });

        services.AddKeyedSingleton<TableClient>("DqlMessages", (sp, _) =>
        {
            var options = sp
                .GetRequiredService<IOptions<AzureStorageOptions>>()
                .Value;

            var tableServiceClient = sp
                .GetRequiredService<TableServiceClient>();

            return tableServiceClient.GetTableClient(
                options.DqlMessageTableName);
        });

        services.AddSingleton<IDqlMessageStore, AzureTableDqlMessageStore>();

        return services;
    }
}