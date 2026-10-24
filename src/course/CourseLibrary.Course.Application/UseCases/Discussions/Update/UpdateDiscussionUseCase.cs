using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Discussions;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Persistence.Repositories.Abstractions;

namespace CourseLibrary.Course.Application.UseCases.Discussions.Update;

public interface IUpdateDiscussionUseCase
{
    Task<Result<DiscussionResponse>> ExecuteAsync(
        string discussionId,
        string courseId,
        string title,
        string description,
        CancellationToken cancellationToken = default);
}

internal sealed class UpdateDiscussionUseCase(
    IDiscussionRepository repository,
    IValidator<UpdateDiscussionRequest> validator,
    ILogger<UpdateDiscussionUseCase> logger) : IUpdateDiscussionUseCase
{
    public Task<Result<DiscussionResponse>> ExecuteAsync(
        string discussionId,
        string courseId,
        string title,
        string description,
        CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "discussions.update", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new UpdateDiscussionRequest(discussionId, courseId, title, description), cancellationToken));
            var id = UseCaseInput.DiscussionId(discussionId);
            var parentCourseId = UseCaseInput.CourseId(courseId);
            var existing = await repository.GetByIdAsync(id, parentCourseId, cancellationToken)
                ?? throw new DataNotFoundException($"Discussion '{discussionId}' was not found.");
            var updated = existing with { Title = title, Description = description, UpdatedAt = DateTimeOffset.UtcNow };
            logger.UpdatingDiscussion(updated.Id.ToString());
            await repository.UpsertAsync(updated, cancellationToken);
            return CourseMapper.ToResponse(updated);
        });
}
