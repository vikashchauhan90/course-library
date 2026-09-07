using CourseLibrary.Domain;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses;

internal static partial class CourseOperationLogs
{
    [LoggerMessage(EventId = EventIds.Courses.GetCourses + 20, Level = LogLevel.Information, Message = "Searching courses with term {SearchTerm}, author {AuthorId}, includeDeleted {IncludeDeleted}, includeRetired {IncludeRetired}")]
    public static partial void SearchingCourses(this ILogger logger, string? searchTerm, string? authorId, bool includeDeleted, bool includeRetired);

    [LoggerMessage(EventId = EventIds.Courses.GetCourses + 21, Level = LogLevel.Information, Message = "Retrieved {CourseCount} courses")]
    public static partial void CoursesSearched(this ILogger logger, int courseCount);

    [LoggerMessage(EventId = EventIds.Courses.CreateCourse + 20, Level = LogLevel.Information, Message = "Persisting course {CourseId} for author {AuthorId}")]
    public static partial void PersistingCourse(this ILogger logger, string courseId, string authorId);

    [LoggerMessage(EventId = EventIds.Courses.UpdateCourse + 20, Level = LogLevel.Information, Message = "Updating course {CourseId}")]
    public static partial void UpdatingCourse(this ILogger logger, string courseId);

    [LoggerMessage(EventId = EventIds.Courses.DeleteCourse + 20, Level = LogLevel.Information, Message = "Deleting course {CourseId}")]
    public static partial void DeletingCourse(this ILogger logger, string courseId);

    [LoggerMessage(EventId = EventIds.Courses.DeleteCourse + 21, Level = LogLevel.Warning, Message = "Course {CourseId} was not found for deletion")]
    public static partial void CourseNotFoundForDeletion(this ILogger logger, string courseId);

    [LoggerMessage(EventId = EventIds.Courses.CreateCourse + 22, Level = LogLevel.Information, Message = "Course {CourseId} created")]
    public static partial void CourseCreatedEvent(this ILogger logger, string courseId);

    [LoggerMessage(EventId = EventIds.Courses.UpdateCourse + 22, Level = LogLevel.Information, Message = "Course {CourseId} updated")]
    public static partial void CourseUpdatedEvent(this ILogger logger, string courseId);

    [LoggerMessage(EventId = EventIds.Courses.DeleteCourse + 22, Level = LogLevel.Information, Message = "Course {CourseId} deleted")]
    public static partial void CourseDeletedEvent(this ILogger logger, string courseId);

    [LoggerMessage(EventId = EventIds.Courses.CreateCourse + 23, Level = LogLevel.Information, Message = "Creating course audit entry for {CourseId}")]
    public static partial void CreatingCourseAudit(this ILogger logger, string courseId);

    [LoggerMessage(EventId = EventIds.Courses.UpdateCourse + 23, Level = LogLevel.Information, Message = "Creating course update audit entry for {CourseId}")]
    public static partial void CreatingCourseUpdateAudit(this ILogger logger, string courseId);

    [LoggerMessage(EventId = EventIds.Courses.DeleteCourse + 23, Level = LogLevel.Information, Message = "Creating course delete audit entry for {CourseId}")]
    public static partial void CreatingCourseDeleteAudit(this ILogger logger, string courseId);
}
