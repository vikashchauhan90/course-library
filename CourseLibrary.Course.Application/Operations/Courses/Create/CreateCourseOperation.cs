using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.Events;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.Operations.Courses;
using CourseLibrary.Course.Domain.Repositories;

namespace CourseLibrary.Course.Application.Operations.Courses.Create;

public interface ICreateCourseOperation
{
    Task<Result<CourseResponse>> ExecuteAsync(
        CreateCourseRequest request,
        string actorId,
        CancellationToken cancellationToken = default);
}

internal sealed class CreateCourseOperation(
    ICourseRepository repository,
    ICourseEventPublisher eventPublisher,
    IValidator<CreateCourseOperationRequest> validator,
    ILogger<CreateCourseOperation> logger) : ICreateCourseOperation
{
    public Task<Result<CourseResponse>> ExecuteAsync(
        CreateCourseRequest request,
        string actorId,
        CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "courses.create", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(
                new CreateCourseOperationRequest(request, actorId), cancellationToken));

            var input = request!;
            var now = DateTimeOffset.UtcNow;
            var course = new CourseLibrary.Course.Domain.Entities.Course
            {
                Id = CourseId.New(),
                Title = input.Title,
                Description = input.Description,
                AuthorId = input.AuthorId,
                AuthorName = input.AuthorName,
                CreatedAt = now,
                UpdatedAt = now
            };

            logger.PersistingCourse(course.Id, course.AuthorId.ToString());
            await repository.UpsertAsync(course, cancellationToken);
            await eventPublisher.PublishAsync(course, actorId, CourseEventType.Created, now,
            [
                CourseAuditEntry.Added(nameof(course.Title), course.Title),
                CourseAuditEntry.Added(nameof(course.Description), course.Description),
                CourseAuditEntry.Added(nameof(course.AuthorId), course.AuthorId.Value),
                CourseAuditEntry.Added(nameof(course.AuthorName), course.AuthorName),
                CourseAuditEntry.Added(nameof(course.CreatedAt), course.CreatedAt),
                CourseAuditEntry.Added(nameof(course.UpdatedAt), course.UpdatedAt)
            ], cancellationToken);

            return CourseMapper.ToResponse(course);
        });
}
