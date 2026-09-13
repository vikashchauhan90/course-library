using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Infrastructure.Cosmos.Configurations;

internal class DiscussionConfiguration : ICosmosDocumentConfiguration<Discussion>
{
    public string ContainerName => "discussions";
    public string PartitionKeyPath => "/courseId";

    public string GetPartitionKey(Discussion document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.CourseId.ToString();
    }
}
