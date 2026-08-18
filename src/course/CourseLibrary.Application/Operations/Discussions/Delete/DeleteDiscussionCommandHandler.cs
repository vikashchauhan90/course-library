using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Discussions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Discussions.Delete;

public sealed class DeleteDiscussionCommandHandler(IDiscussionRepository repository, ILogger<DeleteDiscussionCommandHandler> logger, IEventDispatcher eventDispatcher) : IHandler<DeleteDiscussionCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteDiscussionCommand command, CancellationToken ct)
    {
        logger.DeletingDiscussion(command.DiscussionId);
        if (!await repository.DeleteAsync(command.DiscussionId, command.CourseId, ct))
        {
            logger.DiscussionNotFoundForDeletion(command.DiscussionId);
            return false;
        }
        await eventDispatcher.PublishAsync(new DiscussionDeletedEvent(command.DiscussionId, command.CourseId), ct);
        return true;
    }
}
