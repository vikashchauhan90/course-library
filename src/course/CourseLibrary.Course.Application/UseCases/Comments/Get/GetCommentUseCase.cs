using CourseLibrary.Course.Application.Core;
using CourseLibrary.Platform.Exceptions;
using CourseLibrary.Platform.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Contracts.DTOs.Courses.Comments;
using CourseLibrary.Course.Persistence.Repositories.Courses.Comments;

namespace CourseLibrary.Course.Application.UseCases.Comments.Get;

public interface IGetCommentUseCase
{
    Task<Result<CommentResponse>> ExecuteAsync(string commentId, string courseId, CancellationToken cancellationToken = default);
}

internal sealed class GetCommentUseCase(
    ICommentRepository repository,
    IValidator<GetCommentRequest> validator,
    ILogger<GetCommentUseCase> logger) : IGetCommentUseCase
{
    public Task<Result<CommentResponse>> ExecuteAsync(string commentId, string courseId, CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "comments.get", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new GetCommentRequest(commentId, courseId), cancellationToken));
            var comment = await repository.GetByIdAsync(
                UseCaseInput.CommentId(commentId),
                UseCaseInput.CourseId(courseId),
                cancellationToken);
            return comment ?? throw new DataNotFoundException($"Comment '{commentId}' was not found.");
        });
}
