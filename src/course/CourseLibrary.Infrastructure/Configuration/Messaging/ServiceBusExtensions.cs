using Azure.Messaging.ServiceBus;
using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Infrastructure.Messaging;
using CourseLibrary.Infrastructure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Infrastructure.Configuration.Messaging;

public static class ServiceBusExtensions
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

        services.AddScoped<IEventPublisher, ServiceBusEventPublisher>();

        return services;
    }
}
