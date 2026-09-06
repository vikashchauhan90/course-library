namespace CourseLibrary.Application.Operations.Courses;

using CourseLibrary.Application.Operations.Authors;

/// <summary>
/// Response model for Course operations.
/// </summary>
public sealed record CourseResponse(
    string Id,
    string Title,
    string Description,
    string AuthorId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    AuthorResponse? Author);
