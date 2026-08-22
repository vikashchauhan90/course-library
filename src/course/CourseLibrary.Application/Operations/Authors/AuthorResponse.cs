namespace CourseLibrary.Application.Operations.Authors;

/// <summary>
/// Base response model for Author operations.
/// </summary>
public sealed record AuthorResponse(
    string Id,
    string Name,
    string? Bio,
    string? Website,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
