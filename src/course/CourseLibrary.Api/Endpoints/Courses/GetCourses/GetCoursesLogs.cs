namespace CourseLibrary.Api.Endpoints.Courses.GetCourses;

public static partial class GetCoursesLogs
{
    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Information,
        Message = "Getting all courses")]
    public static partial void GettingAllCourses(
        this ILogger logger);

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Information,
        Message = "Retrieved {CourseCount} courses")]
    public static partial void CoursesRetrieved(
        this ILogger logger,
        int courseCount);
}
