using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;


namespace CourseLibrary.Course.Persistence.Data.Configurations;

internal class CourseConfiguration: ICosmosDocumentConfiguration<Domain.Entities.Course>
{
    public string ContainerName => "courses";
    public string PartitionKeyPath => "/authorId";

    public string GetPartitionKey(Domain.Entities.Course document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.AuthorId.ToString();
    }
}
