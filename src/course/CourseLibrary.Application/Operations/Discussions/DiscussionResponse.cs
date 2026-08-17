namespace CourseLibrary.Application.Operations.Discussions;

/// <summary>
/// Response model for Discussion operations.
/// </summary>
public sealed record DiscussionResponse(
    string Id,
    string CourseId,
    string Title,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt);
