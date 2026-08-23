using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.ServiceDiscovery;

namespace CourseLibrary.Infrastructure.Configuration.Resilience;

public static class HttpClientResilienceExtensions
{
    public static IServiceCollection AddCourseLibraryHttpResilience(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ServiceDiscoveryOptions>(
           static options =>
           {
               options.AllowAllSchemes = false;
               options.AllowedSchemes = ["https"];
           });

        services.AddServiceDiscovery();
        services.ConfigureHttpClientDefaults(
              builder =>
            {
                builder.AddStandardResilienceHandler();
                builder.AddServiceDiscovery();
            });

        services.AddCourseLibraryResilience();
        return services;
    }
}