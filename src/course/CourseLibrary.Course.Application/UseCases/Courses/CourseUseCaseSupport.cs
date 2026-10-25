using CourseLibrary.Course.Contracts.DTOs.Courses;
using CourseLibrary.Course.Contracts.DTOs.Courses.Audits;
using CourseLibrary.Course.Contracts.ValueObjects;
using CourseLibrary.Platform.Exceptions;

namespace CourseLibrary.Course.Application.UseCases.Courses;

internal static class CourseAuthorization
{
    public static void EnsureAuthor(CourseResponse course, string actorId)
    {
        if (!string.Equals(course.AuthorId, actorId, StringComparison.Ordinal))
        {
            throw new ForbiddenException("The current user is not authorized to modify this course.");
        }
    }
}

internal static class CourseAuditEntry
{
    public static AuditEntry Added(string name, object? value)
        => new()
        {
            Action = AuditAction.Add.ToString(),
            Name = name,
            Value = value,
            ValueTypeName = value?.GetType().Name
        };

    public static AuditEntry Updated(string name, object? value)
        => new()
        {
            Action = AuditAction.Updated.ToString(),
            Name = name,
            Value = value,
            ValueTypeName = value?.GetType().Name
        };

    public static void AddIfChanged<T>(ICollection<AuditEntry> changes, string name, T previous, T current)
    {
        if (!EqualityComparer<T>.Default.Equals(previous, current))
        {
            changes.Add(Updated(name, current));
        }
    }
}
