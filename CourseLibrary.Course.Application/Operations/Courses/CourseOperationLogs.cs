using CourseLibrary.Course.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.Operations.Courses;

internal static partial class CourseOperationLogs
{
    [LoggerMessage(7110, LogLevel.Information, "Searching courses with searchTermPresent {SearchTermPresent}, author {AuthorId}, includeDeleted {IncludeDeleted}, includeRetired {IncludeRetired}.")]
    public static partial void SearchingCourses(
        this ILogger logger,
        bool searchTermPresent,
        string? authorId,
        bool includeDeleted,
        bool includeRetired);

    [LoggerMessage(7111, LogLevel.Information, "Retrieved {CourseCount} courses.")]
    public static partial void CoursesSearched(this ILogger logger, int courseCount);

    [LoggerMessage(7120, LogLevel.Information, "Persisting course {CourseId} for author {AuthorId}.")]
    public static partial void PersistingCourse(this ILogger logger, CourseId courseId, string authorId);

    [LoggerMessage(7121, LogLevel.Information, "Updating course {CourseId}.")]
    public static partial void UpdatingCourse(this ILogger logger, CourseId courseId);

    [LoggerMessage(7122, LogLevel.Information, "Retiring course {CourseId}.")]
    public static partial void RetiringCourse(this ILogger logger, CourseId courseId);

    [LoggerMessage(7123, LogLevel.Information, "Deleting course {CourseId}.")]
    public static partial void DeletingCourse(this ILogger logger, CourseId courseId);

    [LoggerMessage(7124, LogLevel.Warning, "Course {CourseId} was not found for deletion.")]
    public static partial void CourseNotFoundForDeletion(this ILogger logger, CourseId courseId);

    [LoggerMessage(7125, LogLevel.Information, "Published course event {EventKind} for course {CourseId}.")]
    public static partial void CourseEventPublished(
        this ILogger logger,
        CourseEventType eventKind,
        CourseId courseId);
}
