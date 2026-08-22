namespace CourseLibrary.Application.Operations.Courses;

/// <summary>
/// Response model for Course operations.
/// </summary>
public sealed record CourseResponse(
    string Id,
    string Title,
    string Description,
    string AuthorId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
