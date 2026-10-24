using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.Operations.Courses;
using CourseLibrary.Course.Application.Operations.Comments;
using CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;
using CourseLibrary.Course.Persistence.Data.Configurations;

namespace CourseLibrary.Course.Application.Operations.Comments.Create;

public interface ICreateCommentOperation
{
    Task<Result<CommentResponse>> ExecuteAsync(
        string courseId,
        string authorId,
        string content,
        string? parentCommentId = null,
        CancellationToken cancellationToken = default);
}

internal sealed class CreateCommentOperation(
    ICommentRepository repository,
    IValidator<CreateCommentOperationRequest> validator,
    ILogger<CreateCommentOperation> logger) : ICreateCommentOperation
{
    public Task<Result<CommentResponse>> ExecuteAsync(
        string courseId,
        string authorId,
        string content,
        string? parentCommentId = null,
        CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "comments.create", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(
                new CreateCommentOperationRequest(courseId, authorId, content, parentCommentId), cancellationToken));
            string? parentId = default;
            if (!string.IsNullOrWhiteSpace(parentCommentId))
            {
                parentId = OperationInput.CommentId(parentCommentId, nameof(parentCommentId));
            }
            var now = DateTimeOffset.UtcNow;
            var normalizedCourseId = OperationInput.CourseId(courseId);
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
