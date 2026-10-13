using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.Events;
using CourseLibrary.Course.Domain.Entities;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.Operations.Courses;
using CourseLibrary.Course.Domain.Repositories;

namespace CourseLibrary.Course.Application.Operations.Courses.Update;

public interface IUpdateCourseOperation
{
    Task<Result<CourseResponse>> ExecuteAsync(
        string courseId,
        UpdateCourseRequest request,
        string actorId,
        CancellationToken cancellationToken = default);
}

internal sealed class UpdateCourseOperation(
    ICourseRepository repository,
    ICourseEventPublisher eventPublisher,
    IValidator<UpdateCourseOperationRequest> validator,
    ILogger<UpdateCourseOperation> logger) : IUpdateCourseOperation
{
    public Task<Result<CourseResponse>> ExecuteAsync(
        string courseId,
        UpdateCourseRequest request,
        string actorId,
        CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "courses.update", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(
                new UpdateCourseOperationRequest(courseId, request, actorId), cancellationToken));
            var input = request!;
            var id = OperationInput.CourseId(courseId);
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
