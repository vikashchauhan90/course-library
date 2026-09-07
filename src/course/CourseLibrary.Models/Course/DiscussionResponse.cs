namespace CourseLibrary.Models.Course;

public sealed record DiscussionResponse(
    string Id,
    string CourseId,
    string Title,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);