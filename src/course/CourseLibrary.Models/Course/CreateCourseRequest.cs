namespace CourseLibrary.Models.Course;

public sealed record CreateCourseRequest(
    string Title,
    string Description,
    string AuthorId,
    string AuthorName);
