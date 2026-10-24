using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;
using Microsoft.Azure.Cosmos;

namespace CourseLibrary.Course.Persistence.Data.Configurations;

internal class DiscussionConfiguration : ICosmosDocumentConfiguration<Discussion>
{
    public const string PartitionKeyFormat = CoursePartitionKeyFormats.CourseFormat;
    public string ContainerName => "discussions";
    public string PartitionKeyPath => "/partitionKey";
    public PartitionKey GetPartitionKey(Discussion document)
    {
        return new PartitionKey(document.PartitionKey);
    }
}
