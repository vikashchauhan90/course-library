using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.Operations.Courses;
using CourseLibrary.Course.Domain.Repositories;

namespace CourseLibrary.Course.Application.Operations.Comments.Get;

public interface IGetCommentOperation
{
    Task<Result<CommentResponse>> ExecuteAsync(string commentId, string courseId, CancellationToken cancellationToken = default);
}

internal sealed class GetCommentOperation(
    ICommentRepository repository,
    IValidator<GetCommentRequest> validator,
    ILogger<GetCommentOperation> logger) : IGetCommentOperation
{
    public Task<Result<CommentResponse>> ExecuteAsync(string commentId, string courseId, CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "comments.get", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new GetCommentRequest(commentId, courseId), cancellationToken));
            var comment = await repository.GetByIdAsync(
                OperationInput.CommentId(commentId),
                OperationInput.CourseId(courseId),
                cancellationToken);
            return CourseMapper.ToResponse(comment ?? throw new DataNotFoundException($"Comment '{commentId}' was not found."));
        });
}
