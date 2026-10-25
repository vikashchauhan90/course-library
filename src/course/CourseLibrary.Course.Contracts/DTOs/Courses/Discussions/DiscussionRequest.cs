namespace CourseLibrary.Course.Contracts.DTOs.Courses.Discussions;

public sealed record DiscussionRequest(
    string? CourseId,
    string? Title,
    string? Description);
