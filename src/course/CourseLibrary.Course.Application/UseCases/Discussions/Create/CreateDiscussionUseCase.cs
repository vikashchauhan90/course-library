using CourseLibrary.Course.Application.Core;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
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
    Task<Result<DiscussionResponse>> ExecuteAsync(string courseId, string title, string description, CancellationToken cancellationToken = default);
}

internal sealed class CreateDiscussionUseCase(
    IDiscussionRepository repository,
    IValidator<CreateDiscussionRequest> validator,
    ILogger<CreateDiscussionUseCase> logger) : ICreateDiscussionUseCase
{
    public Task<Result<DiscussionResponse>> ExecuteAsync(string courseId, string title, string description, CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "discussions.create", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new CreateDiscussionRequest(courseId, title, description), cancellationToken));
            var now = DateTimeOffset.UtcNow;
            var normalizedCourseId = UseCaseInput.CourseId(courseId);
            var discussion = new Discussion
            {
                Id = Guid.NewGuid().ToString(),
                CourseId = normalizedCourseId,
                PartitionKey = CoursePartitionKeyFormats.ForCourse(normalizedCourseId),
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
