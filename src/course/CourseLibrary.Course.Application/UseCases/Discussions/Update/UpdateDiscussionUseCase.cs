using CourseLibrary.Course.Application.Core;
using CourseLibrary.Platform.Exceptions;
using CourseLibrary.Platform.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Discussions;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Contracts.DTOs.Courses.Discussions;
using CourseLibrary.Course.Persistence.Repositories.Courses.Discussions;

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
            var request = new UpdateDiscussionRequest(title, description);
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(request, cancellationToken));
            var id = UseCaseInput.DiscussionId(discussionId);
            var parentCourseId = UseCaseInput.CourseId(courseId);
            var existing = await repository.GetByIdAsync(id, parentCourseId, cancellationToken)
                ?? throw new DataNotFoundException($"Discussion '{discussionId}' was not found.");
            var updated = existing with { Title = title, Description = description, UpdatedAt = DateTimeOffset.UtcNow };
            logger.UpdatingDiscussion(updated.Id.ToString());
            await repository.UpsertAsync(updated, cancellationToken);
            return updated;
        });
}
