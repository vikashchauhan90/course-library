using CourseLibrary.Course.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.UseCases.Courses;

internal static partial class CourseUseCaseLogs
{
    [LoggerMessage(7110, LogLevel.Information, "Searching courses with searchTermPresent {SearchTermPresent}, author {AuthorId}, includeDeleted {IncludeDeleted}, includeRetired {IncludeRetired}.", EventName = nameof(SearchingCourses))]
    public static partial void SearchingCourses(
        this ILogger logger,
        bool searchTermPresent,
        string? authorId,
        bool includeDeleted,
        bool includeRetired);

    [LoggerMessage(7111, LogLevel.Information, "Retrieved {CourseCount} courses.", EventName = nameof(CoursesSearched))]
    public static partial void CoursesSearched(this ILogger logger, int courseCount);

    [LoggerMessage(7120, LogLevel.Information, "Persisting course {CourseId} for author {AuthorId}.", EventName = nameof(PersistingCourse))]
    public static partial void PersistingCourse(this ILogger logger, string courseId, string authorId);

    [LoggerMessage(7121, LogLevel.Information, "Updating course {CourseId}.", EventName = nameof(UpdatingCourse))]
    public static partial void UpdatingCourse(this ILogger logger, string courseId);

    [LoggerMessage(7122, LogLevel.Information, "Retiring course {CourseId}.", EventName = nameof(RetiringCourse))]
    public static partial void RetiringCourse(this ILogger logger, string courseId);

    [LoggerMessage(7123, LogLevel.Information, "Deleting course {CourseId}.", EventName = nameof(DeletingCourse))]
    public static partial void DeletingCourse(this ILogger logger, string courseId);

    [LoggerMessage(7124, LogLevel.Warning, "Course {CourseId} was not found for deletion.", EventName = nameof(CourseNotFoundForDeletion))]
    public static partial void CourseNotFoundForDeletion(this ILogger logger, string courseId);

    [LoggerMessage(7125, LogLevel.Information, "Published course event {EventKind} for course {CourseId}.", EventName = nameof(CourseEventPublished))]
    public static partial void CourseEventPublished(
        this ILogger logger,
        CourseEventType eventKind,
        string courseId);
}
