using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.Core;

namespace CourseLibrary.Course.Application.Operations;

internal sealed class DiscussionOperations(
    IDiscussionRepository repository,
    ILogger<DiscussionOperations> logger,
    IValidator<OperationValidationInput> validator) : IDiscussionOperations
{
    public Task<Result<DiscussionResponse>> GetAsync(
        string discussionId,
        string courseId,
        CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(
            logger,
            "discussions.get",
            async () =>
            {
                await OperationValidation.ValidateAsync(
                    validator,
                    cancellationToken,
                    OperationValidation.Identifier(nameof(discussionId), discussionId),
                    OperationValidation.Identifier(nameof(courseId), courseId));
                var id = OperationInput.DiscussionId(discussionId);
                var parentCourseId = OperationInput.CourseId(courseId);
                var discussion = await repository.GetByIdAsync(id, parentCourseId, cancellationToken);
                return CourseMapper.ToResponse(discussion
                    ?? throw new DataNotFoundException($"Discussion '{discussionId}' was not found."));
            });

    public Task<Result<IReadOnlyList<DiscussionResponse>>> GetByCourseAsync(
        string courseId,
        CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(
            logger,
            "discussions.get_by_course",
            async () =>
            {
                await OperationValidation.ValidateAsync(
                    validator,
                    cancellationToken,
                    OperationValidation.Identifier(nameof(courseId), courseId));
                var id = OperationInput.CourseId(courseId);
                var discussions = await repository.GetByCourseAsync(id, cancellationToken);
                return (IReadOnlyList<DiscussionResponse>)discussions
                    .Select(CourseMapper.ToResponse)
                    .ToArray();
            });

    public Task<Result<DiscussionResponse>> CreateAsync(
        string courseId,
        string title,
        string description,
        CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(
            logger,
            "discussions.create",
            async () =>
            {
                await OperationValidation.ValidateAsync(
                    validator,
                    cancellationToken,
                    OperationValidation.Identifier(nameof(courseId), courseId),
                    OperationValidation.Required(nameof(title), title),
                    OperationValidation.Required(nameof(description), description));
                var parentCourseId = OperationInput.CourseId(courseId);

                var now = DateTimeOffset.UtcNow;
                var discussion = new Discussion
                {
                    Id = DiscussionId.New(),
                    CourseId = parentCourseId,
                    Title = title,
                    Description = description,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await repository.UpsertAsync(discussion, cancellationToken);
                return CourseMapper.ToResponse(discussion);
            });

    public Task<Result<DiscussionResponse>> UpdateAsync(
        string discussionId,
        string courseId,
        string title,
        string description,
        CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(
            logger,
            "discussions.update",
            async () =>
            {
                await OperationValidation.ValidateAsync(
                    validator,
                    cancellationToken,
                    OperationValidation.Identifier(nameof(discussionId), discussionId),
                    OperationValidation.Identifier(nameof(courseId), courseId),
                    OperationValidation.Required(nameof(title), title),
                    OperationValidation.Required(nameof(description), description));
                var id = OperationInput.DiscussionId(discussionId);
                var parentCourseId = OperationInput.CourseId(courseId);
                var existing = await repository.GetByIdAsync(id, parentCourseId, cancellationToken)
                    ?? throw new DataNotFoundException($"Discussion '{discussionId}' was not found.");

                var updated = existing with
                {
                    Title = title,
                    Description = description,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                await repository.UpsertAsync(updated, cancellationToken);
                return CourseMapper.ToResponse(updated);
            });

    public Task<Result<bool>> DeleteAsync(
        string discussionId,
        string courseId,
        CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(
            logger,
            "discussions.delete",
            async () =>
            {
                await OperationValidation.ValidateAsync(
                    validator,
                    cancellationToken,
                    OperationValidation.Identifier(nameof(discussionId), discussionId),
                    OperationValidation.Identifier(nameof(courseId), courseId));
                var id = OperationInput.DiscussionId(discussionId);
                var parentCourseId = OperationInput.CourseId(courseId);
                return await repository.DeleteAsync(id, parentCourseId, cancellationToken);
            });
}
