namespace CourseLibrary.Application.Operations.Authors;

/// <summary>
/// Base response model for Author operations.
/// </summary>
public sealed record AuthorResponse(
    string Id,
    string Name,
    string? Bio,
    string? Website,
    DateTime CreatedAt,
    DateTime UpdatedAt);
