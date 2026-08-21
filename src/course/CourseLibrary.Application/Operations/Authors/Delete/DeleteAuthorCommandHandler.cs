using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Authors.Delete;

public sealed class DeleteAuthorCommandHandler(IAuthorRepository repository, IAuthorAuditRepository auditRepository, IRequestContext requestContext, ILogger<DeleteAuthorCommandHandler> logger, IEventDispatcher eventDispatcher) : IHandler<DeleteAuthorCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteAuthorCommand command, CancellationToken ct)
    {
        logger.DeletingAuthor(command.AuthorId);
        var author = await repository.GetByIdAsync(command.AuthorId, ct);
        if (author is null) { logger.AuthorNotFoundForDeletion(command.AuthorId); return false; }
        if (!await repository.DeleteAsync(command.AuthorId, ct)) return false;
        await auditRepository.AddAsync(new AuthorAuditEntry { Id = Guid.NewGuid().ToString(), AuthorId = author.Id, Action = AuditAction.Deleted, Name = author.Name, Bio = author.Bio, Website = author.Website, OccurredAt = DateTime.UtcNow, ActorId = requestContext.UserId, CorrelationId = requestContext.CorrelationId }, ct);
        await eventDispatcher.PublishAsync(new AuthorDeletedEvent(command.AuthorId), ct);
        return true;
    }
}
