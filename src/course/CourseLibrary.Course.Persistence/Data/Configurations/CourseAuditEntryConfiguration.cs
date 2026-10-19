using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;

namespace CourseLibrary.Course.Persistence.Data.Configurations;

internal class CourseAuditEntryConfiguration: ICosmosDocumentConfiguration<CourseAuditEntry>
{
    public string ContainerName => "course-audit";
    public string PartitionKeyPath => "/courseId";

    public string GetPartitionKey(CourseAuditEntry document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.CourseId.ToString();
    }
}
