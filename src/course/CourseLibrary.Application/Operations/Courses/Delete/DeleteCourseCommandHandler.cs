using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Delete;

public sealed class DeleteCourseCommandHandler(ICourseRepository repository, ICourseAuditRepository auditRepository, IRequestContext requestContext, ILogger<DeleteCourseCommandHandler> logger, IEventDispatcher eventDispatcher) : IHandler<DeleteCourseCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteCourseCommand command, CancellationToken ct)
    {
        logger.DeletingCourse(command.CourseId);
        var course = await repository.GetByIdAsync(command.CourseId, command.PartitionKey, ct);
        if (course is null) { logger.CourseNotFoundForDeletion(command.CourseId); return false; }
        if (!await repository.DeleteAsync(command.CourseId, command.PartitionKey, ct)) return false;
        await auditRepository.AddAsync(new CourseAuditEntry { Id = Guid.NewGuid().ToString(), CourseId = course.Id, Action = AuditAction.Deleted, AuthorId = course.AuthorId, Title = course.Title, Description = course.Description, OccurredAt = DateTime.UtcNow, ActorId = requestContext.UserId, CorrelationId = requestContext.CorrelationId }, ct);
        await eventDispatcher.PublishAsync(new CourseDeletedEvent(command.CourseId, command.PartitionKey), ct);
        return true;
    }
}
