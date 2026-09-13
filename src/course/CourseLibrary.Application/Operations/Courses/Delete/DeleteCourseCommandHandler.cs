using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Domain.Events;
using CourseLibrary.Domain.ValueObjects;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using DomainErrors = CourseLibrary.Domain.Exceptions;

namespace CourseLibrary.Application.Operations.Courses.Delete;

public sealed class DeleteCourseCommandHandler(
    ICourseRepository repository,
    IRequestContext requestContext,
    ILogger<DeleteCourseCommandHandler> logger,
    IEventDispatcher eventDispatcher)
    : IHandler<DeleteCourseCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteCourseCommand command, CancellationToken ct)
    {
        CourseId courseId = (CourseId)command.CourseId;
        logger.DeletingCourse(courseId);

        var userId = requestContext.UserId
            ?? throw new DomainErrors.UnauthorizedException();

        var course = await repository.GetByIdAsync(courseId, ct);
        if (course is null)
        {
            logger.CourseNotFoundForDeletion(courseId);
            return false;
        }
        
        if (!string.Equals(course.AuthorId.ToString(), userId, StringComparison.Ordinal))
        {
            logger.LogWarning(
                "User '{UserId}' attempted to delete course '{CourseId}' without proper authorization.",
                userId,
                command.CourseId);
            throw new DomainErrors.UnauthorizedAccessException();
        }

        if (course.DeletedAt is not null)
        {
            logger.LogWarning(
                "User '{UserId}' attempted to deleting course '{CourseId}' which is already deleted.",
                userId,
                command.CourseId);
            return false;
        }

        var deletedAt = DateTimeOffset.UtcNow;
        var deleted = course with
        {
            DeletedAt = deletedAt,
            UpdatedAt = deletedAt
        };

        await repository.UpsertAsync(deleted, ct);

        var courseEvent = new CourseEvent
        {
            EventId = Guid.NewGuid().ToString(),
            CourseId = course.Id,
            ActorId = requestContext.UserId ?? "unknown",
            OccurredAt = deletedAt,
            EventType = CourseEventType.Deleted,
            ChangedProperties = [
                    new() { Action = AuditAction.Add, Name = nameof(deleted.DeletedAt), Value = deleted.DeletedAt },
                    new() { Action = AuditAction.Updated, Name = nameof(deleted.UpdatedAt), Value = deleted.UpdatedAt }
                ],
        };

        await eventDispatcher.PublishAsync(
            courseEvent,
            ct);
        return true;
    }
}
