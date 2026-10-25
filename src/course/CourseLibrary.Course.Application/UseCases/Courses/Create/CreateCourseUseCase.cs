using CourseLibrary.Course.Application.Core;
using CourseLibrary.Platform.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Contracts.ValueObjects;
using CourseLibrary.Course.Contracts.DTOs.Courses;
using CourseLibrary.Course.Persistence.Repositories.Courses;
using CourseLibrary.Course.Contracts.DTOs.Courses.Comments;
using CourseLibrary.Course.Contracts.DTOs.Courses.Discussions;

namespace CourseLibrary.Course.Application.UseCases.Courses.Create;

public interface ICreateCourseUseCase
{
    Task<Result<CourseResponse>> ExecuteAsync(
        CourseRequest request,
        string actorId,
        CancellationToken cancellationToken = default);
}

internal sealed class CreateCourseUseCase(
    ICourseRepository repository,
    ICourseEventPublisher eventPublisher,
    IValidator<CourseRequest> validator,
    ILogger<CreateCourseUseCase> logger) : ICreateCourseUseCase
{
    public Task<Result<CourseResponse>> ExecuteAsync(
        CourseRequest request,
        string actorId,
        CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "courses.create", async () =>
        {
            ArgumentNullException.ThrowIfNull(request);
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(request, cancellationToken));
            UseCaseInput.Required(actorId, nameof(actorId));

            var input = request!;
            var now = DateTimeOffset.UtcNow;
            var course = new CourseResponse(
                Guid.NewGuid().ToString(),
                input.Title,
                input.Description,
                input.AuthorId,
                input.AuthorName,
                now,
                now,
                null,
                null,
                Array.Empty<CommentResponse>(),
                Array.Empty<DiscussionResponse>());

            logger.PersistingCourse(course.Id, course.AuthorId.ToString());
            await repository.UpsertAsync(course, cancellationToken);
            await eventPublisher.PublishAsync(course, actorId, CourseEventType.Created, now,
            [
                CourseAuditEntry.Added(nameof(course.Title), course.Title),
                CourseAuditEntry.Added(nameof(course.Description), course.Description),
                CourseAuditEntry.Added(nameof(course.AuthorId), course.AuthorId),
                CourseAuditEntry.Added(nameof(course.AuthorName), course.AuthorName),
                CourseAuditEntry.Added(nameof(course.CreatedAt), course.CreatedAt),
                CourseAuditEntry.Added(nameof(course.UpdatedAt), course.UpdatedAt)
            ], cancellationToken);

            return course;
        });
}
