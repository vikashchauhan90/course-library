using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Comments;
using CourseLibrary.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Comments.Delete;

public sealed class DeleteCommentCommandHandler(ICommentRepository repository, ILogger<DeleteCommentCommandHandler> logger) : IHandler<DeleteCommentCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteCommentCommand command, CancellationToken ct)
    {
        var commentId = (CommentId)Guid.Parse(command.CommentId);
        var courseId = (CourseId)Guid.Parse(command.CourseId);
        logger.DeletingComment(command.CommentId);
        if (!await repository.DeleteAsync(commentId, courseId, ct))
        {
            logger.CommentNotFoundForDeletion(command.CommentId);
            return false;
        }
        return true;
    }
}
