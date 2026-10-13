using CourseLibrary.Course.Application.Core;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Data.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.Operations.Discussions;

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
            var id = OperationInput.DiscussionId(discussionId);
            logger.DeletingDiscussion(id.ToString());
            var deleted = await repository.DeleteAsync(
                id,
                OperationInput.CourseId(courseId),
                cancellationToken);
            if (!deleted)
            {
                logger.DiscussionNotFoundForDeletion(id.ToString());
            }

            return deleted;
        });
}
