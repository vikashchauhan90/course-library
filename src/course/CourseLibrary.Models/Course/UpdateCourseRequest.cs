namespace CourseLibrary.Models.Course;

public sealed record UpdateCourseRequest(
       string Title,
    string Description,
    string AuthorId,
    string AuthorName);