using CourseLibrary.Course.Application.Core;
using CourseLibrary.Platform.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Comments;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Contracts.DTOs.Courses.Comments;
using CourseLibrary.Course.Persistence.Repositories.Courses;
using CourseLibrary.Course.Persistence.Repositories.Courses.Comments;

namespace CourseLibrary.Course.Application.UseCases.Comments.Create;

public interface ICreateCommentUseCase
{
    Task<Result<CommentResponse>> ExecuteAsync(
        CommentRequest request,
        CancellationToken cancellationToken = default);
}

internal sealed class CreateCommentUseCase(
    ICommentRepository repository,
    IValidator<CommentRequest> validator,
    ILogger<CreateCommentUseCase> logger) : ICreateCommentUseCase
{
    public Task<Result<CommentResponse>> ExecuteAsync(
        CommentRequest request,
        CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "comments.create", async () =>
        {
            ArgumentNullException.ThrowIfNull(request);
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(request, cancellationToken));
            string? parentId = default;
            if (!string.IsNullOrWhiteSpace(request.ParentCommentId))
            {
                parentId = UseCaseInput.CommentId(request.ParentCommentId, nameof(request.ParentCommentId));
            }
            var now = DateTimeOffset.UtcNow;
            var normalizedCourseId = UseCaseInput.CourseId(request.CourseId!);
            var comment = new CommentResponse(
                Guid.NewGuid().ToString(),
                normalizedCourseId,
                request.AuthorId!,
                request.Content!,
                parentId,
                now,
                now,
                null);
            logger.PersistingComment(comment.Id.ToString(), request.CourseId!);
            await repository.UpsertAsync(comment, cancellationToken);
            return comment;
        });
}
