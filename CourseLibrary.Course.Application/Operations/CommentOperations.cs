using CourseLibrary.Course.Application.Observability;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.Operations;

public sealed class CommentOperations(
    ICommentRepository repository,
    ILogger<CommentOperations> logger)
{
    public Task<CommentResponse?> GetAsync(
        string commentId,
        string courseId,
        CancellationToken cancellationToken = default)
    {
        var id = OperationInput.CommentId(commentId);
        var parentCourseId = OperationInput.CourseId(courseId);

        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "comments.get",
            async () =>
            {
                var comment = await repository.GetByIdAsync(id, parentCourseId, cancellationToken);
                return comment is null ? null : CourseMapper.ToResponse(comment);
            });
    }

    public Task<IReadOnlyList<CommentResponse>> GetByCourseAsync(
        string courseId,
        CancellationToken cancellationToken = default)
    {
        var id = OperationInput.CourseId(courseId);

        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "comments.get_by_course",
            async () =>
            {
                var comments = await repository.GetByCourseAsync(id, cancellationToken);
                return (IReadOnlyList<CommentResponse>)comments
                    .Select(CourseMapper.ToResponse)
                    .ToArray();
            });
    }

    public Task<CommentResponse> CreateAsync(
        string courseId,
        string authorId,
        string content,
        string? parentCommentId = null,
        CancellationToken cancellationToken = default)
    {
        var parentCourseId = OperationInput.CourseId(courseId);
        OperationInput.Required(authorId, nameof(authorId));
        OperationInput.Required(content, nameof(content));
        CommentId? parsedParentId = null;
        if (!string.IsNullOrWhiteSpace(parentCommentId))
        {
            parsedParentId = OperationInput.CommentId(parentCommentId, nameof(parentCommentId));
        }

        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "comments.create",
            async () =>
            {
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
    }

    public Task<CommentResponse> UpdateAsync(
        string commentId,
        string courseId,
        string content,
        CancellationToken cancellationToken = default)
    {
        var id = OperationInput.CommentId(commentId);
        var parentCourseId = OperationInput.CourseId(courseId);
        OperationInput.Required(content, nameof(content));

        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "comments.update",
            async () =>
            {
                var existing = await repository.GetByIdAsync(id, parentCourseId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Comment '{commentId}' was not found.");

                var updated = existing with
                {
                    Content = content,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                await repository.UpsertAsync(updated, cancellationToken);
                return CourseMapper.ToResponse(updated);
            });
    }

    public Task<bool> DeleteAsync(
        string commentId,
        string courseId,
        CancellationToken cancellationToken = default)
    {
        var id = OperationInput.CommentId(commentId);
        var parentCourseId = OperationInput.CourseId(courseId);

        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "comments.delete",
            () => repository.DeleteAsync(id, parentCourseId, cancellationToken));
    }
}
