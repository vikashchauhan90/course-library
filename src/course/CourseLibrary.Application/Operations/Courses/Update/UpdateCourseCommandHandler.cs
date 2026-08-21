using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Application.Operations.Courses;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Update;

public sealed class UpdateCourseCommandHandler(ICourseRepository repository, ICourseAuditRepository auditRepository, IRequestContext requestContext, ILogger<UpdateCourseCommandHandler> logger, IEventDispatcher eventDispatcher) : IHandler<UpdateCourseCommand, CourseResponse>
{
    public async Task<CourseResponse> HandleAsync(UpdateCourseCommand command, CancellationToken ct)
    {
        logger.UpdatingCourse(command.Id);
        var existing = await repository.GetByIdAsync(command.Id, command.AuthorId, ct);
        if (existing is null)
        {
            throw new KeyNotFoundException($"Course '{command.Id}' not found");
        }

        var updated = existing with
        {
            Title = command.Title,
            Description = command.Description,
            UpdatedAt = DateTime.UtcNow
        };

        await repository.UpsertAsync(updated, ct);
        await auditRepository.AddAsync(new CourseAuditEntry { Id = Guid.NewGuid().ToString(), CourseId = updated.Id, Action = AuditAction.Updated, AuthorId = updated.AuthorId, Title = updated.Title, Description = updated.Description, OccurredAt = updated.UpdatedAt, ActorId = requestContext.UserId, CorrelationId = requestContext.CorrelationId }, ct);
        await eventDispatcher.PublishAsync(new CourseUpdatedEvent(updated.Id, updated.AuthorId, updated.Title, updated.UpdatedAt), ct);
        return CourseMapper.ToResponse(updated);
    }
}
