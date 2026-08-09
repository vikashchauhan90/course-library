namespace CourseLibrary.Infrastructure.Cosmos;

public sealed class CosmosOptions
{
    public required string AccountEndpoint { get; init; }
    public required string AccountKey { get; init; }
    public required string DatabaseName { get; init; }
    public required string CoursesContainer { get; init; }
    public required string AuthorsContainer { get; init; }
    public required string CommentsContainer { get; init; }
    public required string DiscussionsContainer { get; init; }
}
