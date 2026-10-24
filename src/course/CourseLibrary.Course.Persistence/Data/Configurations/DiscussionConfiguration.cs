using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;
using System.Globalization;

namespace CourseLibrary.Course.Persistence.Data.Configurations;

internal class DiscussionConfiguration : ICosmosDocumentConfiguration<Discussion>
{
    public const string PartitionKeyFormat = CoursePartitionKeyFormats.CourseFormat;
    public string ContainerName => "discussions";
    public string PartitionKeyPath => "/courseId";

    public string GetPartitionKey(Discussion document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return CreatePartitionKey(document.CourseId.Value);
    }

    public string CreatePartitionKey(params ReadOnlySpan<object?> args)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            PartitionKeyFormat,
            args);
    }
}
