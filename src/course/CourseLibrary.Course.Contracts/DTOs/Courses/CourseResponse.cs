using CourseLibrary.Course.Contracts.DTOs.Courses.Comments;
using CourseLibrary.Course.Contracts.DTOs.Courses.Discussions;

namespace CourseLibrary.Course.Contracts.DTOs.Courses;

public sealed record CourseResponse(
    string Id,
    string Title,
    string Description,
    string AuthorId,
    string AuthorName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? RetiredAt,
    DateTimeOffset? DeletedAt,
    IReadOnlyList<CommentResponse> Comments,
    IReadOnlyList<DiscussionResponse> Discussions);
