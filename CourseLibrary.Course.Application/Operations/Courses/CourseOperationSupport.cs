using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseEntity = CourseLibrary.Course.Domain.Entities.Course;

namespace CourseLibrary.Course.Application.Operations.Courses;

internal static class CourseAuthorization
{
    public static void EnsureAuthor(CourseEntity course, string actorId)
    {
        if (!string.Equals(course.AuthorId.Value, actorId, StringComparison.Ordinal))
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
            Action = AuditAction.Add,
            Name = name,
            Value = value,
            ValueTypeName = value?.GetType().Name
        };

    public static AuditEntry Updated(string name, object? value)
        => new()
        {
            Action = AuditAction.Updated,
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
