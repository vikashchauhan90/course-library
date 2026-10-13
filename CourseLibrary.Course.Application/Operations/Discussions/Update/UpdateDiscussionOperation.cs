using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.Operations.Courses;

namespace CourseLibrary.Course.Application.Operations.Discussions.Update;

public interface IUpdateDiscussionOperation
{
    Task<Result<DiscussionResponse>> ExecuteAsync(
        string discussionId,
        string courseId,
        string title,
        string description,
        CancellationToken cancellationToken = default);
}

internal sealed class UpdateDiscussionOperation(
    IDiscussionRepository repository,
    IValidator<UpdateDiscussionRequest> validator,
    ILogger<UpdateDiscussionOperation> logger) : IUpdateDiscussionOperation
{
    public Task<Result<DiscussionResponse>> ExecuteAsync(
        string discussionId,
        string courseId,
        string title,
        string description,
        CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "discussions.update", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new UpdateDiscussionRequest(discussionId, courseId, title, description), cancellationToken));
            var id = OperationInput.DiscussionId(discussionId);
            var parentCourseId = OperationInput.CourseId(courseId);
            var existing = await repository.GetByIdAsync(id, parentCourseId, cancellationToken)
                ?? throw new DataNotFoundException($"Discussion '{discussionId}' was not found.");
            var updated = existing with { Title = title, Description = description, UpdatedAt = DateTimeOffset.UtcNow };
            await repository.UpsertAsync(updated, cancellationToken);
            return CourseMapper.ToResponse(updated);
        });
}
