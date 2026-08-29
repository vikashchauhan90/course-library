namespace CourseLibrary.Infrastructure.Configuration.AzureStorage;

public sealed class AzureStorageOptions
{
    public const string SectionName = "AzureStorage";
    public string? ConnectionString { get; init; }
}