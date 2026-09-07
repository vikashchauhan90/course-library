using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Domain.Events;
using CourseLibrary.Domain.Abstractions;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

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
        logger.DeletingCourse(command.CourseId);
        var course = await repository.GetByIdAsync(command.CourseId, ct);
        if (course is null) { logger.CourseNotFoundForDeletion(command.CourseId); return false; }
        if (!string.Equals(course.AuthorId, command.AuthorId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException();
        var deletedAt = DateTimeOffset.UtcNow;
        var deleted = course with { DeletedAt = deletedAt, UpdatedAt = deletedAt };
        await repository.UpsertAsync(deleted, ct);

      await eventDispatcher.PublishAsync(
            new CourseDeletedEvent(
                command.CourseId,
                Guid.NewGuid().ToString(),
                requestContext.UserId ?? "unknown",
                deletedAt,
                [
                    new() { Action = AuditAction.Deleted, Name = nameof(deleted.DeletedAt), Value = deleted.DeletedAt },
                    new() { Action = AuditAction.Deleted, Name = nameof(deleted.UpdatedAt), Value = deleted.UpdatedAt }
                ]),
            ct);
        return true;
    }
}
