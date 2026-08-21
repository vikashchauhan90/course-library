using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Comments;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Comments.Delete;

public sealed class DeleteCommentCommandHandler(ICommentRepository repository, ILogger<DeleteCommentCommandHandler> logger, IEventDispatcher eventDispatcher) : IHandler<DeleteCommentCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteCommentCommand command, CancellationToken ct)
    {
        logger.DeletingComment(command.CommentId);
        if (!await repository.DeleteAsync(command.CommentId, command.CourseId, ct))
        {
            logger.CommentNotFoundForDeletion(command.CommentId);
            return false;
        }
        return true;
    }
}
