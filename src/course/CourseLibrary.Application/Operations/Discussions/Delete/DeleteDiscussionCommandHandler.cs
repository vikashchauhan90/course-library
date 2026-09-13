using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Discussions;
using CourseLibrary.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Discussions.Delete;

public sealed class DeleteDiscussionCommandHandler(IDiscussionRepository repository, ILogger<DeleteDiscussionCommandHandler> logger) : IHandler<DeleteDiscussionCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteDiscussionCommand command, CancellationToken ct)
    {
        var discussionId = (DiscussionId)Guid.Parse(command.DiscussionId);
        var courseId = (CourseId)Guid.Parse(command.CourseId);
        logger.DeletingDiscussion(command.DiscussionId);
        if (!await repository.DeleteAsync(discussionId, courseId, ct))
        {
            logger.DiscussionNotFoundForDeletion(command.DiscussionId);
            return false;
        }
        return true;
    }
}
