using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;
using System.Globalization;

namespace CourseLibrary.Course.Persistence.Data.Configurations;

internal class CourseAuditEntryConfiguration: ICosmosDocumentConfiguration<CourseAuditEntry>
{
    public const string PartitionKeyFormat = CoursePartitionKeyFormats.CourseFormat;
    public string ContainerName => "course-audit";
    public string PartitionKeyPath => "/courseId";

    public string GetPartitionKey(CourseAuditEntry document)
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
