using CourseLibrary.Course.Application.Core;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Comments;
using CourseLibrary.Course.Persistence.Repositories.Abstractions;

namespace CourseLibrary.Course.Application.UseCases.Comments.Delete;

public interface IDeleteCommentUseCase
{
    Task<Result<bool>> ExecuteAsync(string commentId, string courseId, CancellationToken cancellationToken = default);
}

internal sealed class DeleteCommentUseCase(
    ICommentRepository repository,
    IValidator<DeleteCommentRequest> validator,
    ILogger<DeleteCommentUseCase> logger) : IDeleteCommentUseCase
{
    public Task<Result<bool>> ExecuteAsync(string commentId, string courseId, CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "comments.delete", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new DeleteCommentRequest(commentId, courseId), cancellationToken));
            var id = UseCaseInput.CommentId(commentId);
            logger.DeletingComment(id.ToString());
            var deleted = await repository.DeleteAsync(
                id,
                UseCaseInput.CourseId(courseId),
                cancellationToken);
            if (!deleted)
            {
                logger.CommentNotFoundForDeletion(id.ToString());
            }

            return deleted;
        });
}
