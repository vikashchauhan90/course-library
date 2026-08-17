namespace CourseLibrary.Api.Endpoints.Courses.CreateCourse;

public static partial class CreateCourseLogs
{
    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Information,
        Message = "Creating course {Title}")]
    public static partial void CreatingCourse(
        this ILogger logger,
        string title);

    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Information,
        Message = "Course {CourseId} created")]
    public static partial void CourseCreated(
        this ILogger logger,
        string courseId);
}
