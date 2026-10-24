using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;
using Microsoft.Azure.Cosmos;

namespace CourseLibrary.Course.Persistence.Data.Configurations;

internal class CourseAuditEntryConfiguration: ICosmosDocumentConfiguration<CourseAuditEntry>
{
    public string ContainerName => "course-audit";
    public string PartitionKeyPath => "/partitionKey";
    public PartitionKey GetPartitionKey(CourseAuditEntry document)
    {
        return new PartitionKey(document.PartitionKey);
    }
}
