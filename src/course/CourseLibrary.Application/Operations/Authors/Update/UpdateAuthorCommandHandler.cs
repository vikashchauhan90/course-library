using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Application.Operations.Authors;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Authors.Update;

public sealed class UpdateAuthorCommandHandler(IAuthorRepository repository, IAuthorAuditRepository auditRepository, IRequestContext requestContext, ILogger<UpdateAuthorCommandHandler> logger, IEventDispatcher eventDispatcher) : IHandler<UpdateAuthorCommand, AuthorResponse>
{
    public async Task<AuthorResponse> HandleAsync(UpdateAuthorCommand command, CancellationToken ct)
    {
        logger.UpdatingAuthor(command.Id);
        var existing = await repository.GetByIdAsync(command.Id, ct);
        if (existing is null)
            throw new KeyNotFoundException($"Author '{command.Id}' not found");

        var updated = existing with
        {
            Name = command.Name,
            Bio = command.Bio,
            Website = command.Website,
            UpdatedAt = DateTime.UtcNow
        };

        await repository.UpsertAsync(updated, ct);
        await auditRepository.AddAsync(new AuthorAuditEntry { Id = Guid.NewGuid().ToString(), AuthorId = updated.Id, Action = AuditAction.Updated, Name = updated.Name, Bio = updated.Bio, Website = updated.Website, OccurredAt = updated.UpdatedAt, ActorId = requestContext.UserId, CorrelationId = requestContext.CorrelationId }, ct);
        await eventDispatcher.PublishAsync(
            new AuthorUpdatedEvent(
                updated.Id,
                updated.Name,
                Guid.NewGuid().ToString(),
                updated.UpdatedAt),
            ct);
        return AuthorMapper.ToResponse(updated);
    }
}
