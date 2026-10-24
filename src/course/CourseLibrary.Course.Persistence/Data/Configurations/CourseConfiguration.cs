using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;
using Microsoft.Azure.Cosmos;


namespace CourseLibrary.Course.Persistence.Data.Configurations;

internal class CourseConfiguration : ICosmosDocumentConfiguration<Course.Domain.Entities.Course>
{
    public string ContainerName => "courses";
    public string PartitionKeyPath => "/partitionKey";

    public PartitionKey GetPartitionKey(Course.Domain.Entities.Course document)
    {
        return new PartitionKey(document.PartitionKey);
    }

}
