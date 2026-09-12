using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Infrastructure.Cosmos.Configurations;

internal class CourseAuditEntryConfiguration: ICosmosDocumentConfiguration<CourseAuditEntry>
{
    public string ContainerName => "course-audit";
    public string PartitionKeyPath => "/courseId";

    public string GetPartitionKey(CourseAuditEntry document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.CourseId;
    }
}
