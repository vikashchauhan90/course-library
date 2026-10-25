using CourseLibrary.Course.Application.Core;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Comments;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Contracts.DTOs.Courses.Comments;
using CourseLibrary.Course.Persistence.Repositories.Courses.Comments;

namespace CourseLibrary.Course.Application.UseCases.Comments.Update;

public interface IUpdateCommentUseCase
{
    Task<Result<CommentResponse>> ExecuteAsync(string commentId, string courseId, string content, CancellationToken cancellationToken = default);
}

internal sealed class UpdateCommentUseCase(
    ICommentRepository repository,
    IValidator<UpdateCommentRequest> validator,
    ILogger<UpdateCommentUseCase> logger) : IUpdateCommentUseCase
{
    public Task<Result<CommentResponse>> ExecuteAsync(string commentId, string courseId, string content, CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "comments.update", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new UpdateCommentRequest(commentId, courseId, content), cancellationToken));
            var id = UseCaseInput.CommentId(commentId);
            var parentCourseId = UseCaseInput.CourseId(courseId);
            var existing = await repository.GetByIdAsync(id, parentCourseId, cancellationToken)
                ?? throw new DataNotFoundException($"Comment '{commentId}' was not found.");
            var updated = existing with { Content = content, UpdatedAt = DateTimeOffset.UtcNow };
            logger.UpdatingComment(updated.Id.ToString());
            await repository.UpsertAsync(updated, cancellationToken);
            return CourseMapper.ToResponse(updated);
        });
}
