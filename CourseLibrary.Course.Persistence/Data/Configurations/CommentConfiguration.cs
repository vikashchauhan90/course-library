using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;

namespace CourseLibrary.Course.Persistence.Data.Configurations;

internal sealed class CommentConfiguration
    : ICosmosDocumentConfiguration<Comment>
{
    public  string ContainerName => "comments";
    public  string PartitionKeyPath => "/courseId";

    public string GetPartitionKey(Comment document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.CourseId.ToString();
    }
}