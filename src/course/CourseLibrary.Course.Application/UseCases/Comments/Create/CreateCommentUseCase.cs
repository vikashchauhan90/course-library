using CourseLibrary.Course.Application.Core;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
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
        string courseId,
        string authorId,
        string content,
        string? parentCommentId = null,
        CancellationToken cancellationToken = default);
}

internal sealed class CreateCommentUseCase(
    ICommentRepository repository,
    IValidator<CreateCommentUseCaseRequest> validator,
    ILogger<CreateCommentUseCase> logger) : ICreateCommentUseCase
{
    public Task<Result<CommentResponse>> ExecuteAsync(
        string courseId,
        string authorId,
        string content,
        string? parentCommentId = null,
        CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "comments.create", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(
                new CreateCommentUseCaseRequest(courseId, authorId, content, parentCommentId), cancellationToken));
            string? parentId = default;
            if (!string.IsNullOrWhiteSpace(parentCommentId))
            {
                parentId = UseCaseInput.CommentId(parentCommentId, nameof(parentCommentId));
            }
            var now = DateTimeOffset.UtcNow;
            var normalizedCourseId = UseCaseInput.CourseId(courseId);
            var comment = new Comment
            {
                Id = Guid.NewGuid().ToString(),
                CourseId = normalizedCourseId,
                PartitionKey = CoursePartitionKeyFormats.ForCourse(normalizedCourseId),
                AuthorId = authorId,
                Content = content,
                ParentCommentId = parentId,
                CreatedAt = now,
                UpdatedAt = now
            };
            logger.PersistingComment(comment.Id.ToString(), courseId);
            await repository.UpsertAsync(comment, cancellationToken);
            return CourseMapper.ToResponse(comment);
        });
}
