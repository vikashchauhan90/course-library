namespace CourseLibrary.Infrastructure.Cosmos.Configurations;

public sealed class CosmosOptions
{
    public const string SectionName = "Cosmos";

    public required string AccountEndpoint { get; init; }

    public required string AccountKey { get; init; }

    public required string DatabaseName { get; init; }
}