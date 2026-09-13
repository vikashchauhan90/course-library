using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Infrastructure.Cosmos.Configurations;

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