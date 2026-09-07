namespace CourseLibrary.Models.Course;

public sealed record CreateCourseRequest(
    string Title,
    string Description);
