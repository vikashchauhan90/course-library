using CourseLibrary.Course.Application.Core;
using CourseLibrary.Platform.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Discussions;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Contracts.DTOs.Courses.Discussions;
using CourseLibrary.Course.Persistence.Repositories.Courses;
using CourseLibrary.Course.Persistence.Repositories.Courses.Discussions;

namespace CourseLibrary.Course.Application.UseCases.Discussions.Create;

public interface ICreateDiscussionUseCase
{
    Task<Result<DiscussionResponse>> ExecuteAsync(DiscussionRequest request, CancellationToken cancellationToken = default);
}

internal sealed class CreateDiscussionUseCase(
    IDiscussionRepository repository,
    IValidator<DiscussionRequest> validator,
    ILogger<CreateDiscussionUseCase> logger) : ICreateDiscussionUseCase
{
    public Task<Result<DiscussionResponse>> ExecuteAsync(DiscussionRequest request, CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "discussions.create", async () =>
        {
            ArgumentNullException.ThrowIfNull(request);
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(request, cancellationToken));
            var now = DateTimeOffset.UtcNow;
            var normalizedCourseId = UseCaseInput.CourseId(request.CourseId!);
            var discussion = new DiscussionResponse(
                Guid.NewGuid().ToString(),
                normalizedCourseId,
                request.Title!,
                request.Description!,
                now,
                now);
            logger.PersistingDiscussion(discussion.Id.ToString(), request.CourseId!);
            await repository.UpsertAsync(discussion, cancellationToken);
            return discussion;
        });
}
