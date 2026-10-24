using Microsoft.Azure.Cosmos;

namespace CourseLibrary.Platform.Persistence.CosmosDB.Configurations;

public interface ICosmosDocumentConfiguration<TDocument>
    where TDocument : class
{
    string ContainerName { get; }
    string PartitionKeyPath { get; }

    PartitionKey GetPartitionKey(TDocument document);
}