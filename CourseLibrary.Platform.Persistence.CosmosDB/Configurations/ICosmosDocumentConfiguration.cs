namespace CourseLibrary.Platform.Persistence.CosmosDB.Configurations;

public interface ICosmosDocumentConfiguration
{
    string ContainerName { get; }
    string PartitionKeyPath { get; }
}
public interface ICosmosDocumentConfiguration<TDocument>
    : ICosmosDocumentConfiguration
    where TDocument : class
{
    string GetPartitionKey(TDocument document);
}