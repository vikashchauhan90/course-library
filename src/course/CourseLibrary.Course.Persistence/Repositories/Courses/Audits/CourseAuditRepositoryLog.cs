using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Persistence.Repositories.Courses.Audits;

internal static partial class CourseAuditRepositoryLog
{
    [LoggerMessage(
        EventId = 4101,
        Level = LogLevel.Information,
        Message = "Course audit entry written for course {CourseId}. Cosmos status: {StatusCode}; request charge: {RequestCharge} RU.",
        EventName = "AuditEntryWritten")]
    public static partial void AuditEntryWritten(
        this ILogger<CourseAuditRepository> logger,
        string courseId,
        System.Net.HttpStatusCode statusCode,
        double requestCharge);

    [LoggerMessage(
        EventId = 4102,
        Level = LogLevel.Warning,
        Message = "Course audit write cancelled for course {CourseId}.",
        EventName = "AuditWriteCancelled")]
    public static partial void AuditWriteCancelled(
        this ILogger<CourseAuditRepository> logger,
        string courseId);

    [LoggerMessage(
        EventId = 4103,
        Level = LogLevel.Error,
        Message = "Failed to write course audit entry for course {CourseId}.",
        EventName = "AuditWriteFailed")]
    public static partial void AuditWriteFailed(
        this ILogger<CourseAuditRepository> logger,
        Exception exception,
        string courseId);
}