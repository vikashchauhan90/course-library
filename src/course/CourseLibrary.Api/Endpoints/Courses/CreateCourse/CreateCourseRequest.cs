namespace CourseLibrary.Api.Endpoints.Courses.CreateCourse;

public sealed record CreateCourseRequest(string Title, string Description, string AuthorId);
