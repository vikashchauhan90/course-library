using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;
using System.Globalization;


namespace CourseLibrary.Course.Persistence.Data.Configurations;

internal class CourseConfiguration : ICosmosDocumentConfiguration<Domain.Entities.Course>
{
    public const string PartitionKeyFormat = CoursePartitionKeyFormats.AuthorFormat;
    public string ContainerName => "courses";
    public string PartitionKeyPath => "/authorId";

    public string GetPartitionKey(Domain.Entities.Course document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return CreatePartitionKey(document.AuthorId.Value);
    }
    public string CreatePartitionKey(params ReadOnlySpan<object?> args)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            PartitionKeyFormat,
            args);
    }
}
