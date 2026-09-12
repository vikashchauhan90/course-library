namespace CourseLibrary.Infrastructure.Cosmos.Configurations;

public interface ICosmosDocumentConfiguration<TDocument>
    where TDocument : class
{
    string ContainerName { get; }
    string PartitionKeyPath { get; }
    string GetPartitionKey(TDocument document);
}