namespace CourseLibrary.Course.Contracts.DTOs.Courses;

public sealed record CourseRequest(
    string Title,
    string Description,
    string AuthorId,
    string AuthorName);
