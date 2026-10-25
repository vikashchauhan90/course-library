namespace CourseLibrary.Course.Contracts.DTOs.Courses.Discussions;

public sealed record UpdateDiscussionRequest(
    string? Title,
    string? Description);
