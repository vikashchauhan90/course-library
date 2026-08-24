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

        services.AddSingleton(sp =>
        {
            var options = sp
                .GetRequiredService<IOptions<ServiceBusOptions>>()
                .Value;

            return new ServiceBusClient(
                options.ConnectionString);
        });

        services.AddScoped<IEventRouter, EventRouter>();
        services.AddScoped<IEventPublisher, ServiceBusEventPublisher>();

        return services;
    }
}
