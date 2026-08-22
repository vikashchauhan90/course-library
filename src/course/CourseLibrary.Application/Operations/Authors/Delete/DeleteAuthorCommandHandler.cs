using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Authors.Delete;

public sealed class DeleteAuthorCommandHandler(
    IAuthorRepository repository,
    IRequestContext requestContext,
    IEventDispatcher eventDispatcher,
    ILogger<DeleteAuthorCommandHandler> logger) : IHandler<DeleteAuthorCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteAuthorCommand command, CancellationToken ct)
    {
        logger.DeletingAuthor(command.AuthorId);
        var author = await repository.GetByIdAsync(command.AuthorId, ct);
        if (author is null) { logger.AuthorNotFoundForDeletion(command.AuthorId); return false; }
        if (!await repository.DeleteAsync(command.AuthorId, ct)) return false;
        await eventDispatcher.PublishAsync(
            new AuthorDeletedEvent(
                command.AuthorId,
                Guid.NewGuid().ToString(),
                requestContext.UserId ?? "unknown",
                DateTimeOffset.UtcNow),
            ct);
        return true;
    }
}
