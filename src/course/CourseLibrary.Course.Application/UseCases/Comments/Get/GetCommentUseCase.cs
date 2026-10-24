using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;
using CourseLibrary.Course.Application.UseCases.Courses;

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
            return CourseMapper.ToResponse(comment ?? throw new DataNotFoundException($"Comment '{commentId}' was not found."));
        });
}
