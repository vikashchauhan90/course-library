namespace CourseLibrary.Course.Contracts.DTOs;

public sealed record DiscussionResponse(
    string Id,
    string CourseId,
    string Title,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);