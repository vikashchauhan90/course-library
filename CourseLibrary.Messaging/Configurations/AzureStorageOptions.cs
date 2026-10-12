
namespace CourseLibrary.Messaging.Configurations;

public sealed class AzureStorageOptions
{
    public const string KeyedServiceName = "DqlMessages";
    public const string SectionName = "AzureStorage";
    public string? ConnectionString { get; init; }

    public string DqlMessageTableName { get; init; } = "DqlMessages";
}