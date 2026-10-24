using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;
using Microsoft.Azure.Cosmos;

namespace CourseLibrary.Course.Persistence.Data.Configurations;

internal sealed class CommentConfiguration
    : ICosmosDocumentConfiguration<Comment>
{
    public  string ContainerName => "comments";
    public  string PartitionKeyPath => "/partitionKey";

    public PartitionKey GetPartitionKey(Comment document)
    {
        return new PartitionKey(document.PartitionKey);
    }

}