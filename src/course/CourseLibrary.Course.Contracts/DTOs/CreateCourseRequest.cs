namespace CourseLibrary.Course.Contracts.DTOs;

public sealed record CreateCourseRequest(
    string Title,
    string Description,
    string AuthorId,
    string AuthorName);
