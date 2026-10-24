using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Persistence.Repositories.Abstractions;

namespace CourseLibrary.Course.Application.UseCases.Discussions.Get;

public interface IGetDiscussionUseCase
{
    Task<Result<DiscussionResponse>> ExecuteAsync(string discussionId, string courseId, CancellationToken cancellationToken = default);
}

internal sealed class GetDiscussionUseCase(
    IDiscussionRepository repository,
    IValidator<GetDiscussionRequest> validator,
    ILogger<GetDiscussionUseCase> logger) : IGetDiscussionUseCase
{
    public Task<Result<DiscussionResponse>> ExecuteAsync(string discussionId, string courseId, CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "discussions.get", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new GetDiscussionRequest(discussionId, courseId), cancellationToken));
            var discussion = await repository.GetByIdAsync(
                UseCaseInput.DiscussionId(discussionId),
                UseCaseInput.CourseId(courseId),
                cancellationToken);
            return CourseMapper.ToResponse(discussion ?? throw new DataNotFoundException($"Discussion '{discussionId}' was not found."));
        });
}
