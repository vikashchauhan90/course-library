using CourseLibrary.Domain.Entities;


namespace CourseLibrary.Course.Persistence.Data.Configurations;

internal class CourseConfiguration: ICosmosDocumentConfiguration<Course>
{
    public string ContainerName => "courses";
    public string PartitionKeyPath => "/authorId";

    public string GetPartitionKey(Course document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.AuthorId.ToString();
    }
}
