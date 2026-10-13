using CourseLibrary.Course.Application.Core;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;

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
            return await repository.DeleteAsync(
                OperationInput.CommentId(commentId),
                OperationInput.CourseId(courseId),
                cancellationToken);
        });
}
