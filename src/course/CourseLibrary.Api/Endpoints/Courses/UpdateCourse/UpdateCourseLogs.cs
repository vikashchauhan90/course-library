namespace CourseLibrary.Api.Endpoints.Courses.UpdateCourse;

public static partial class UpdateCourseLogs
{
    [LoggerMessage(
        EventId = 2006,
        Level = LogLevel.Information,
        Message = "Updating course {CourseId}")]
    public static partial void UpdatingCourse(
        this ILogger logger,
        string courseId);

    [LoggerMessage(
        EventId = 2007,
        Level = LogLevel.Information,
        Message = "Course {CourseId} updated")]
    public static partial void CourseUpdated(
        this ILogger logger,
        string courseId);
}
