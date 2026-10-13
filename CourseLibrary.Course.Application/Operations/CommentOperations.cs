using CourseLibrary.Course.Application.Observability;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.Operations;

internal sealed class CommentOperations(
    ICommentRepository repository,
    ILogger<CommentOperations> logger,
    IValidator<OperationValidationInput> validator) : ICommentOperations
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
                await OperationValidation.ValidateAsync(
                    validator,
                    cancellationToken,
                    OperationValidation.Identifier(nameof(commentId), commentId),
                    OperationValidation.Identifier(nameof(courseId), courseId));
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
                await OperationValidation.ValidateAsync(
                    validator,
                    cancellationToken,
                    OperationValidation.Identifier(nameof(courseId), courseId));
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
                await OperationValidation.ValidateAsync(
                    validator,
                    cancellationToken,
                    OperationValidation.Identifier(nameof(courseId), courseId),
                    OperationValidation.Required(nameof(authorId), authorId),
                    OperationValidation.Required(nameof(content), content),
                    OperationValidation.OptionalIdentifier(nameof(parentCommentId), parentCommentId));
                var parentCourseId = OperationInput.CourseId(courseId);
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
                await OperationValidation.ValidateAsync(
                    validator,
                    cancellationToken,
                    OperationValidation.Identifier(nameof(commentId), commentId),
                    OperationValidation.Identifier(nameof(courseId), courseId),
                    OperationValidation.Required(nameof(content), content));
                var id = OperationInput.CommentId(commentId);
                var parentCourseId = OperationInput.CourseId(courseId);
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
                await OperationValidation.ValidateAsync(
                    validator,
                    cancellationToken,
                    OperationValidation.Identifier(nameof(commentId), commentId),
                    OperationValidation.Identifier(nameof(courseId), courseId));
                var id = OperationInput.CommentId(commentId);
                var parentCourseId = OperationInput.CourseId(courseId);
                return await repository.DeleteAsync(id, parentCourseId, cancellationToken);
            });
}
