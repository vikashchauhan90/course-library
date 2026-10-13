using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.Operations.Courses;
using CourseLibrary.Course.Domain.Repositories;

namespace CourseLibrary.Course.Application.Operations.Discussions.Get;

public interface IGetDiscussionOperation
{
    Task<Result<DiscussionResponse>> ExecuteAsync(string discussionId, string courseId, CancellationToken cancellationToken = default);
}

internal sealed class GetDiscussionOperation(
    IDiscussionRepository repository,
    IValidator<GetDiscussionRequest> validator,
    ILogger<GetDiscussionOperation> logger) : IGetDiscussionOperation
{
    public Task<Result<DiscussionResponse>> ExecuteAsync(string discussionId, string courseId, CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "discussions.get", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new GetDiscussionRequest(discussionId, courseId), cancellationToken));
            var discussion = await repository.GetByIdAsync(
                OperationInput.DiscussionId(discussionId),
                OperationInput.CourseId(courseId),
                cancellationToken);
            return CourseMapper.ToResponse(discussion ?? throw new DataNotFoundException($"Discussion '{discussionId}' was not found."));
        });
}
