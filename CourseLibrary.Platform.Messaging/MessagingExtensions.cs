using Azure.Data.Tables;
using Azure.Messaging.ServiceBus;
using CourseLibrary.Platform.Messaging.Abstractions;
using CourseLibrary.Platform.Messaging.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Platform.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddCourseLibraryServiceBus(
       this IServiceCollection services,
       IConfiguration configuration)
    {
        services
           .AddOptions<ServiceBusOptions>()
           .Bind(configuration.GetSection(ServiceBusOptions.SectionName))
           .ValidateDataAnnotations()
           .ValidateOnStart();

        services
            .AddOptions<AzureStorageOptions>()
            .Bind(configuration.GetSection(AzureStorageOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "AzureStorage:ConnectionString must be configured.")
            .ValidateOnStart();

        services.AddSingleton<ServiceBusClient>(sp =>
        {
            var options = sp
                .GetRequiredService<IOptions<ServiceBusOptions>>()
                .Value;

            var clientOptions = new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    MaxDelay = TimeSpan.FromSeconds(10),
                    TryTimeout = TimeSpan.FromSeconds(10)
                }
            };

            return new ServiceBusClient(
                options.ConnectionString,
                clientOptions);
        });

        services.AddSingleton<TableServiceClient>(sp =>
        {
            var options = sp
                .GetRequiredService<IOptions<AzureStorageOptions>>()
                .Value;

            return new TableServiceClient(options.ConnectionString);
        });

        services.AddScoped<IEventPublisher, ServiceBusEventPublisher>();


        services.AddKeyedSingleton<TableClient>(AzureStorageOptions.KeyedServiceName, (sp, _) =>
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
