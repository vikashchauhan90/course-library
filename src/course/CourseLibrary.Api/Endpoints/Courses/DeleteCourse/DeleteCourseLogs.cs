namespace CourseLibrary.Api.Endpoints.Courses.DeleteCourse;

public static partial class DeleteCourseLogs
{
    [LoggerMessage(
        EventId = 2008,
        Level = LogLevel.Information,
        Message = "Deleting course {CourseId}")]
    public static partial void DeletingCourse(
        this ILogger logger,
        string courseId);

    [LoggerMessage(
        EventId = 2009,
        Level = LogLevel.Information,
        Message = "Course {CourseId} deleted")]
    public static partial void CourseDeleted(
        this ILogger logger,
        string courseId);

    [LoggerMessage(
        EventId = 2010,
        Level = LogLevel.Warning,
        Message = "Course {CourseId} not found for deletion")]
    public static partial void CourseNotFoundForDeletion(
        this ILogger logger,
        string courseId);
}
