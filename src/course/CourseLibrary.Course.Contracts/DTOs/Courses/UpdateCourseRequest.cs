namespace CourseLibrary.Course.Contracts.DTOs.Courses;

public sealed record UpdateCourseRequest(
    string? Title,
    string? Description);
