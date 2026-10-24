using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Platform.Persistence.CosmosDB.Configurations;
using System.Globalization;

namespace CourseLibrary.Course.Persistence.Data.Configurations;

internal sealed class CommentConfiguration
    : ICosmosDocumentConfiguration<Comment>
{
    public const string PartitionKeyFormat = "#COURSE#{0}";
    public  string ContainerName => "comments";
    public  string PartitionKeyPath => "/courseId";

    public string GetPartitionKey(Comment document)
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