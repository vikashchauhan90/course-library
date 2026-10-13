using CourseLibrary.Course.Application.Observability;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.Operations;

public sealed class DiscussionOperations(
    IDiscussionRepository repository,
    ILogger<DiscussionOperations> logger)
{
    public Task<DiscussionResponse?> GetAsync(
        string discussionId,
        string courseId,
        CancellationToken cancellationToken = default)
    {
        var id = OperationInput.DiscussionId(discussionId);
        var parentCourseId = OperationInput.CourseId(courseId);

        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "discussions.get",
            async () =>
            {
                var discussion = await repository.GetByIdAsync(id, parentCourseId, cancellationToken);
                return discussion is null ? null : CourseMapper.ToResponse(discussion);
            });
    }

    public Task<IReadOnlyList<DiscussionResponse>> GetByCourseAsync(
        string courseId,
        CancellationToken cancellationToken = default)
    {
        var id = OperationInput.CourseId(courseId);

        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "discussions.get_by_course",
            async () =>
            {
                var discussions = await repository.GetByCourseAsync(id, cancellationToken);
                return (IReadOnlyList<DiscussionResponse>)discussions
                    .Select(CourseMapper.ToResponse)
                    .ToArray();
            });
    }

    public Task<DiscussionResponse> CreateAsync(
        string courseId,
        string title,
        string description,
        CancellationToken cancellationToken = default)
    {
        var parentCourseId = OperationInput.CourseId(courseId);
        OperationInput.Required(title, nameof(title));
        OperationInput.Required(description, nameof(description));

        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "discussions.create",
            async () =>
            {
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
    }

    public Task<DiscussionResponse> UpdateAsync(
        string discussionId,
        string courseId,
        string title,
        string description,
        CancellationToken cancellationToken = default)
    {
        var id = OperationInput.DiscussionId(discussionId);
        var parentCourseId = OperationInput.CourseId(courseId);
        OperationInput.Required(title, nameof(title));
        OperationInput.Required(description, nameof(description));

        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "discussions.update",
            async () =>
            {
                var existing = await repository.GetByIdAsync(id, parentCourseId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Discussion '{discussionId}' was not found.");

                var updated = existing with
                {
                    Title = title,
                    Description = description,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                await repository.UpsertAsync(updated, cancellationToken);
                return CourseMapper.ToResponse(updated);
            });
    }

    public Task<bool> DeleteAsync(
        string discussionId,
        string courseId,
        CancellationToken cancellationToken = default)
    {
        var id = OperationInput.DiscussionId(discussionId);
        var parentCourseId = OperationInput.CourseId(courseId);

        return CourseApplicationOperation.ExecuteAsync(
            logger,
            "discussions.delete",
            () => repository.DeleteAsync(id, parentCourseId, cancellationToken));
    }
}
