using Azure.Storage.Blobs;
using CourseLibrary.Infrastructure.DataProtection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


namespace CourseLibrary.Infrastructure.Configuration.DataProtection;

public static class DataProtectionExtensions
{
    private const string APPLICATION_NAME = "CourseLibrary";
    public static IServiceCollection AddCourseLibraryDataProtection(
            this IServiceCollection services,
            IConfiguration configuration)
    {

        services
           .AddOptions<DataProtectionOptions>()
           .Bind(configuration.GetSection(DataProtectionOptions.SectionName))
           .ValidateDataAnnotations()
           .ValidateOnStart();


        services.AddDataProtection()
                .SetApplicationName(APPLICATION_NAME)
                .PersistKeysToAzureBlobStorage(sp =>
                {
                    var dataProtectionOptions = sp
                     .GetRequiredService<IOptions<DataProtectionOptions>>()
                     .Value;

                    var blobServiceClient = sp
                        .GetRequiredService<BlobServiceClient>();

                    var containerClient = blobServiceClient
                        .GetBlobContainerClient(
                            dataProtectionOptions.KeyContainerName);

                    var blobClient = containerClient.GetBlobClient(
                        dataProtectionOptions.KeyBlobName);

                    return blobClient;
                })
                .SetDefaultKeyLifetime(
            TimeSpan.FromDays(90));

        services.AddSingleton<IDataProtectionService,
            DataProtectionService>();
        return services;

    }
}
