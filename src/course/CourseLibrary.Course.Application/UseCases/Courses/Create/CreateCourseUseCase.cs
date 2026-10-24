using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.Events;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;
using CourseLibrary.Course.Persistence.Data.Configurations;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Application.UseCases.Courses;

namespace CourseLibrary.Course.Application.UseCases.Courses.Create;

public interface ICreateCourseUseCase
{
    Task<Result<CourseResponse>> ExecuteAsync(
        CreateCourseRequest request,
        string actorId,
        CancellationToken cancellationToken = default);
}

internal sealed class CreateCourseUseCase(
    ICourseRepository repository,
    ICourseEventPublisher eventPublisher,
    IValidator<CreateCourseUseCaseRequest> validator,
    ILogger<CreateCourseUseCase> logger) : ICreateCourseUseCase
{
    public Task<Result<CourseResponse>> ExecuteAsync(
        CreateCourseRequest request,
        string actorId,
        CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "courses.create", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(
                new CreateCourseUseCaseRequest(request, actorId), cancellationToken));

            var input = request!;
            var now = DateTimeOffset.UtcNow;
            var course = new Domain.Entities.Course
            {
                Id = Guid.NewGuid().ToString(),
                Title = input.Title,
                Description = input.Description,
                AuthorId = input.AuthorId,
                PartitionKey = CoursePartitionKeyFormats.ForAuthor(input.AuthorId),
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
                CourseAuditEntry.Added(nameof(course.AuthorId), course.AuthorId),
                CourseAuditEntry.Added(nameof(course.AuthorName), course.AuthorName),
                CourseAuditEntry.Added(nameof(course.CreatedAt), course.CreatedAt),
                CourseAuditEntry.Added(nameof(course.UpdatedAt), course.UpdatedAt)
            ], cancellationToken);

            return CourseMapper.ToResponse(course);
        });
}
