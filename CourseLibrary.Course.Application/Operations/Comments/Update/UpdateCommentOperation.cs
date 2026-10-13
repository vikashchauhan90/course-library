using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.Operations.Courses;

namespace CourseLibrary.Course.Application.Operations.Comments.Update;

public interface IUpdateCommentOperation
{
    Task<Result<CommentResponse>> ExecuteAsync(string commentId, string courseId, string content, CancellationToken cancellationToken = default);
}

internal sealed class UpdateCommentOperation(
    ICommentRepository repository,
    IValidator<UpdateCommentRequest> validator,
    ILogger<UpdateCommentOperation> logger) : IUpdateCommentOperation
{
    public Task<Result<CommentResponse>> ExecuteAsync(string commentId, string courseId, string content, CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "comments.update", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new UpdateCommentRequest(commentId, courseId, content), cancellationToken));
            var id = OperationInput.CommentId(commentId);
            var parentCourseId = OperationInput.CourseId(courseId);
            var existing = await repository.GetByIdAsync(id, parentCourseId, cancellationToken)
                ?? throw new DataNotFoundException($"Comment '{commentId}' was not found.");
            var updated = existing with { Content = content, UpdatedAt = DateTimeOffset.UtcNow };
            await repository.UpsertAsync(updated, cancellationToken);
            return CourseMapper.ToResponse(updated);
        });
}
