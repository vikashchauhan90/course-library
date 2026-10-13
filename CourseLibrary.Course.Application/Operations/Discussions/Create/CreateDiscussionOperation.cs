using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.Operations.Courses;
using CourseLibrary.Course.Application.Operations.Discussions;
using CourseLibrary.Course.Domain.Repositories;

namespace CourseLibrary.Course.Application.Operations.Discussions.Create;

public interface ICreateDiscussionOperation
{
    Task<Result<DiscussionResponse>> ExecuteAsync(string courseId, string title, string description, CancellationToken cancellationToken = default);
}

internal sealed class CreateDiscussionOperation(
    IDiscussionRepository repository,
    IValidator<CreateDiscussionRequest> validator,
    ILogger<CreateDiscussionOperation> logger) : ICreateDiscussionOperation
{
    public Task<Result<DiscussionResponse>> ExecuteAsync(string courseId, string title, string description, CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "discussions.create", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new CreateDiscussionRequest(courseId, title, description), cancellationToken));
            var now = DateTimeOffset.UtcNow;
            var discussion = new Discussion
            {
                Id = DiscussionId.New(),
                CourseId = OperationInput.CourseId(courseId),
                Title = title,
                Description = description,
                CreatedAt = now,
                UpdatedAt = now
            };
            logger.PersistingDiscussion(discussion.Id.ToString(), courseId);
            await repository.UpsertAsync(discussion, cancellationToken);
            return CourseMapper.ToResponse(discussion);
        });
}
