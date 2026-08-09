using Microsoft.Azure.Cosmos;

namespace CourseLibrary.Infrastructure.Cosmos;

public static class CosmosClientFactory
{
    public static CosmosClient Create(CosmosOptions options)
    {
        var clientOptions = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                IgnoreNullValues = true
            },
            ApplicationName = "CourseLibrary.Infrastructure.Cosmos"
        };

        return new CosmosClient(options.AccountEndpoint, options.AccountKey, clientOptions);
    }
}
