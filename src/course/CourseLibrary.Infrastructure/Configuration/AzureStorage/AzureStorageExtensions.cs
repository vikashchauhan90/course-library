using Azure.Storage.Blobs;
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

            return new BlobServiceClient(options.ConnectionString);
        });

        return services;
    }
}