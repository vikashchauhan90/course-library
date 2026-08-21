using Azure.Identity;
using Azure.Messaging.ServiceBus;
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
                options.FullyQualifiedNamespace,
                new DefaultAzureCredential());
        });

        return services;
    }
}
