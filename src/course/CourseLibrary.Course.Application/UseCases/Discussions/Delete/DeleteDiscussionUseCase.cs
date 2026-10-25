using CourseLibrary.Course.Application.Core;
using CourseLibrary.Platform.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Discussions;
using CourseLibrary.Course.Persistence.Repositories.Courses.Discussions;

namespace CourseLibrary.Course.Application.UseCases.Discussions.Delete;

public interface IDeleteDiscussionUseCase
{
    Task<Result<bool>> ExecuteAsync(string discussionId, string courseId, CancellationToken cancellationToken = default);
}

internal sealed class DeleteDiscussionUseCase(
    IDiscussionRepository repository,
    IValidator<DeleteDiscussionRequest> validator,
    ILogger<DeleteDiscussionUseCase> logger) : IDeleteDiscussionUseCase
{
    public Task<Result<bool>> ExecuteAsync(string discussionId, string courseId, CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "discussions.delete", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new DeleteDiscussionRequest(discussionId, courseId), cancellationToken));
            var id = UseCaseInput.DiscussionId(discussionId);
            logger.DeletingDiscussion(id.ToString());
            var deleted = await repository.DeleteAsync(
                id,
                UseCaseInput.CourseId(courseId),
                cancellationToken);
            if (!deleted)
            {
                logger.DiscussionNotFoundForDeletion(id.ToString());
            }

            return deleted;
        });
}
