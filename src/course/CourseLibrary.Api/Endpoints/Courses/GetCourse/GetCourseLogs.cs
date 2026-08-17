namespace CourseLibrary.Api.Endpoints.Courses.GetCourse;

public static partial class GetCourseLogs
{
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Getting course {CourseId}")]
    public static partial void GettingCourse(
        this ILogger logger,
        string courseId);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "Course {CourseId} retrieved")]
    public static partial void CourseRetrieved(
        this ILogger logger,
        string courseId);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Warning,
        Message = "Course {CourseId} not found")]
    public static partial void CourseNotFound(
        this ILogger logger,
        string courseId);
}
