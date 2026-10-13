using CourseLibrary.Course.Application.Observability;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.Operations;

public sealed class CommentOperations(
    ICommentRepository repository,
    ILogger<CommentOperations> logger)
{
    public Task<Result<CommentResponse>> GetAsync(
        string commentId,
        string courseId,
        CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(
            logger,
            "comments.get",
            async () =>
            {
                var id = OperationInput.CommentId(commentId);
                var parentCourseId = OperationInput.CourseId(courseId);
                var comment = await repository.GetByIdAsync(id, parentCourseId, cancellationToken);
                return CourseMapper.ToResponse(comment
                    ?? throw new DataNotFoundException($"Comment '{commentId}' was not found."));
            });

    public Task<Result<IReadOnlyList<CommentResponse>>> GetByCourseAsync(
        string courseId,
        CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(
            logger,
            "comments.get_by_course",
            async () =>
            {
                var id = OperationInput.CourseId(courseId);
                var comments = await repository.GetByCourseAsync(id, cancellationToken);
                return (IReadOnlyList<CommentResponse>)comments
                    .Select(CourseMapper.ToResponse)
                    .ToArray();
            });

    public Task<Result<CommentResponse>> CreateAsync(
        string courseId,
        string authorId,
        string content,
        string? parentCommentId = null,
        CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(
            logger,
            "comments.create",
            async () =>
            {
                var parentCourseId = OperationInput.CourseId(courseId);
                OperationInput.Required(authorId, nameof(authorId));
                OperationInput.Required(content, nameof(content));
                CommentId? parsedParentId = null;
                if (!string.IsNullOrWhiteSpace(parentCommentId))
                {
                    parsedParentId = OperationInput.CommentId(parentCommentId, nameof(parentCommentId));
                }

                var now = DateTimeOffset.UtcNow;
                var comment = new Comment
                {
                    Id = CommentId.New(),
                    CourseId = parentCourseId,
                    AuthorId = authorId,
                    Content = content,
                    ParentCommentId = parsedParentId,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await repository.UpsertAsync(comment, cancellationToken);
                return CourseMapper.ToResponse(comment);
            });

    public Task<Result<CommentResponse>> UpdateAsync(
        string commentId,
        string courseId,
        string content,
        CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(
            logger,
            "comments.update",
            async () =>
            {
                var id = OperationInput.CommentId(commentId);
                var parentCourseId = OperationInput.CourseId(courseId);
                OperationInput.Required(content, nameof(content));
                var existing = await repository.GetByIdAsync(id, parentCourseId, cancellationToken)
                    ?? throw new DataNotFoundException($"Comment '{commentId}' was not found.");

                var updated = existing with
                {
                    Content = content,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                await repository.UpsertAsync(updated, cancellationToken);
                return CourseMapper.ToResponse(updated);
            });

    public Task<Result<bool>> DeleteAsync(
        string commentId,
        string courseId,
        CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(
            logger,
            "comments.delete",
            async () =>
            {
                var id = OperationInput.CommentId(commentId);
                var parentCourseId = OperationInput.CourseId(courseId);
                return await repository.DeleteAsync(id, parentCourseId, cancellationToken);
            });
}
