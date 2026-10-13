using CourseLibrary.Course.Application.Core;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.Operations.Discussions.Delete;

public interface IDeleteDiscussionOperation
{
    Task<Result<bool>> ExecuteAsync(string discussionId, string courseId, CancellationToken cancellationToken = default);
}

internal sealed class DeleteDiscussionOperation(
    IDiscussionRepository repository,
    IValidator<DeleteDiscussionRequest> validator,
    ILogger<DeleteDiscussionOperation> logger) : IDeleteDiscussionOperation
{
    public Task<Result<bool>> ExecuteAsync(string discussionId, string courseId, CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "discussions.delete", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new DeleteDiscussionRequest(discussionId, courseId), cancellationToken));
            return await repository.DeleteAsync(
                OperationInput.DiscussionId(discussionId),
                OperationInput.CourseId(courseId),
                cancellationToken);
        });
}
