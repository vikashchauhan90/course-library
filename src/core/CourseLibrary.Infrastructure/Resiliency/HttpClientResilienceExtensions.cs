using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace CourseLibrary.Infrastructure.Resilience;

public static class HttpClientResilienceExtensions
{
    private const string ConfigurationSection = "Resilience:Http";

    public static IServiceCollection AddCourseLibraryHttpResilience(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(ConfigurationSection);

        services
            .AddOptions<HttpStandardResilienceOptions>()
            .Bind(section)
            .Validate(
                options =>
                    options.Retry.MaxRetryAttempts >= 0,
                "Retry.MaxRetryAttempts must be greater than or equal to 0.")
            .Validate(
                options =>
                    options.Retry.MaxRetryAttempts <= 10,
                "Retry.MaxRetryAttempts must not exceed 10.")
            .Configure(options =>
            {
                // Application-wide safety rule.
                options.Retry.DisableForUnsafeHttpMethods();
            })
            .ValidateOnStart();

        services.ConfigureHttpClientDefaults(
            builder =>
            {
                builder.AddStandardResilienceHandler();
            });

        return services;
    }
}