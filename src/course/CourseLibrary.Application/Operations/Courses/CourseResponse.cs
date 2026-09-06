namespace CourseLibrary.Application.Operations.Courses;

using CourseLibrary.Application.Operations.Comments;
using CourseLibrary.Application.Operations.Discussions;

/// <summary>
/// Response model for Course operations.
/// </summary>
public sealed record CourseResponse(
    string Id,
    string Title,
    string Description,
    string AuthorId,
    string AuthorName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<CommentResponse> Comments,
    IReadOnlyList<DiscussionResponse> Discussions);
