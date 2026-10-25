using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.Events;
using CourseLibrary.Course.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Contracts.ValueObjects;
using CourseLibrary.Course.Persistence.Entities;
using CourseLibrary.Course.Contracts.DTOs.Courses;
using CourseLibrary.Course.Persistence.Repositories.Courses;

namespace CourseLibrary.Course.Application.UseCases.Courses.Update;

public interface IUpdateCourseUseCase
{
    Task<Result<CourseResponse>> ExecuteAsync(
        string courseId,
        UpdateCourseRequest request,
        string actorId,
        CancellationToken cancellationToken = default);
}

internal sealed class UpdateCourseUseCase(
    ICourseRepository repository,
    ICourseEventPublisher eventPublisher,
    IValidator<UpdateCourseUseCaseRequest> validator,
    ILogger<UpdateCourseUseCase> logger) : IUpdateCourseUseCase
{
    public Task<Result<CourseResponse>> ExecuteAsync(
        string courseId,
        UpdateCourseRequest request,
        string actorId,
        CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "courses.update", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(
                new UpdateCourseUseCaseRequest(courseId, request, actorId), cancellationToken));
            var input = request!;
            var id = UseCaseInput.CourseId(courseId);
            var course = await repository.GetByIdAsync(id, cancellationToken)
                ?? throw new DataNotFoundException($"Course '{courseId}' was not found.");
            CourseAuthorization.EnsureAuthor(course, actorId);

            var now = DateTimeOffset.UtcNow;
            var updated = course with { Title = input.Title, Description = input.Description, UpdatedAt = now };
            logger.UpdatingCourse(updated.Id);
            await repository.UpsertAsync(updated, cancellationToken);
            var changes = new List<AuditEntry>();
            CourseAuditEntry.AddIfChanged(changes, nameof(updated.Title), course.Title, updated.Title);
            CourseAuditEntry.AddIfChanged(changes, nameof(updated.Description), course.Description, updated.Description);
            if (changes.Count == 0)
            {
                changes.Add(CourseAuditEntry.Updated(nameof(updated.UpdatedAt), updated.UpdatedAt));
            }

            await eventPublisher.PublishAsync(updated, actorId, CourseEventType.Updated, now, changes, cancellationToken);
            return CourseMapper.ToResponse(updated);
        });
}
