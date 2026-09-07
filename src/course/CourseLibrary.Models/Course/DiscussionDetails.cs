namespace CourseLibrary.Models.Course;

public sealed record DiscussionDetails(
    string Id,
    string CourseId,
    string Title,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);