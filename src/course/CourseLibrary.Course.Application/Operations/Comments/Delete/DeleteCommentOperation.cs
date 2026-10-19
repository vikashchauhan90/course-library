using CourseLibrary.Course.Application.Core;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.Operations.Comments;
using CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;

namespace CourseLibrary.Course.Application.Operations.Comments.Delete;

public interface IDeleteCommentOperation
{
    Task<Result<bool>> ExecuteAsync(string commentId, string courseId, CancellationToken cancellationToken = default);
}

internal sealed class DeleteCommentOperation(
    ICommentRepository repository,
    IValidator<DeleteCommentRequest> validator,
    ILogger<DeleteCommentOperation> logger) : IDeleteCommentOperation
{
    public Task<Result<bool>> ExecuteAsync(string commentId, string courseId, CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "comments.delete", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new DeleteCommentRequest(commentId, courseId), cancellationToken));
            var id = OperationInput.CommentId(commentId);
            logger.DeletingComment(id.ToString());
            var deleted = await repository.DeleteAsync(
                id,
                OperationInput.CourseId(courseId),
                cancellationToken);
            if (!deleted)
            {
                logger.CommentNotFoundForDeletion(id.ToString());
            }

            return deleted;
        });
}
