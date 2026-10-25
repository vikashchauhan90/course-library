namespace CourseLibrary.Course.Contracts.DTOs.Courses.Discussions;

public sealed record DiscussionResponse(
    string Id,
    string CourseId,
    string Title,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);