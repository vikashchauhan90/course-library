using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Domain.Abstractions;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Events;
using CourseLibrary.Models.Course;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using DomainErrors = CourseLibrary.Domain.Exceptions;

namespace CourseLibrary.Application.Operations.Courses.Update;

public sealed class UpdateCourseCommandHandler(
    ICourseRepository repository,
    IRequestContext requestContext,
    IEventDispatcher eventDispatcher,
    ILogger<UpdateCourseCommandHandler> logger
    )
    : IHandler<UpdateCourseCommand, CourseResponse?>
{
    public async Task<CourseResponse?> HandleAsync(UpdateCourseCommand command, CancellationToken ct)
    {
        logger.UpdatingCourse(command.Id);

        var userId = requestContext.UserId
            ?? throw new DomainErrors.UnauthorizedException();

        var course = await repository.GetByIdAsync(command.Id, ct);
        if (course is null)
        {
            logger.LogWarning(
                "Course '{CourseId}' not found for update.",
                command.Id);

            return null;
        }

        if (!string.Equals(course.AuthorId, userId, StringComparison.Ordinal))
        {
            logger.LogWarning(
                "User '{UserId}' attempted to update course '{CourseId}' without proper authorization.",
                userId,
                command.Id);
            throw new DomainErrors.UnauthorizedAccessException();
        }

        var updated = course with
        {
            Title = command.Title,
            Description = command.Description,
            UpdatedAt = DateTime.UtcNow
        };

        await repository.UpsertAsync(updated, ct);

        var courseEvent = new CourseEvent
        {
            EventId = Guid.NewGuid().ToString(),
            CourseId = updated.Id,
            ActorId = requestContext.UserId ?? "unknown",
            OccurredAt = updated.UpdatedAt,
            EventType = CourseEventType.Updated,
            ChangedProperties = GetChangedProperties(course, updated),
        };
        await eventDispatcher.PublishAsync(
            courseEvent,
            ct);
        return CourseMapper.ToResponse(updated);
    }

    private static IReadOnlyList<AuditEntry> GetChangedProperties(
        Course previous,
        Course current)
    {
        var changes = new List<AuditEntry>();
        AddIfChanged(changes, nameof(current.Title), previous.Title, current.Title);
        AddIfChanged(changes, nameof(current.Description), previous.Description, current.Description);
        AddIfChanged(changes, nameof(current.AuthorName), previous.AuthorName, current.AuthorName);
        AddIfChanged(changes, nameof(current.RetiredAt), previous.RetiredAt, current.RetiredAt);
        AddIfChanged(changes, nameof(current.DeletedAt), previous.DeletedAt, current.DeletedAt);
        if (changes.Count == 0)
            changes.Add(new AuditEntry { Action = AuditAction.Updated, Name = nameof(current.UpdatedAt), Value = current.UpdatedAt });
        return changes;
    }

    private static void AddIfChanged<T>(ICollection<AuditEntry> changes, string name, T previous, T current)
    {
        if (!EqualityComparer<T>.Default.Equals(previous, current))
            changes.Add(new AuditEntry { Action = AuditAction.Updated, Name = name, Value = current });
    }
}
