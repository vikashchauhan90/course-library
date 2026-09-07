namespace CourseLibrary.Models.Course;

public sealed record UpdateCourseRequest(
   string Title,
   string Description);