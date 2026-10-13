using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;

namespace CourseLibrary.Course.Persistence.Data.Configurations;

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
