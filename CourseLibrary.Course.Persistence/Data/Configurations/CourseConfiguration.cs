using CourseEntity = CourseLibrary.Course.Domain.Entities.Course;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;


namespace CourseLibrary.Course.Persistence.Data.Configurations;

internal class CourseConfiguration: ICosmosDocumentConfiguration<CourseEntity>
{
    public string ContainerName => "courses";
    public string PartitionKeyPath => "/authorId";

    public string GetPartitionKey(CourseEntity document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.AuthorId.ToString();
    }
}
