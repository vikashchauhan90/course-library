using CourseLibrary.Course.Application.Core;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.Events;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Contracts.ValueObjects;
using CourseLibrary.Course.Persistence.Repositories.Abstractions;

namespace CourseLibrary.Course.Application.UseCases.Courses.Delete;

public interface IDeleteCourseUseCase
{
    Task<Result<bool>> ExecuteAsync(string courseId, string actorId, CancellationToken cancellationToken = default);
}

internal sealed class DeleteCourseUseCase(
    ICourseRepository repository,
    ICourseEventPublisher eventPublisher,
    IValidator<DeleteCourseRequest> validator,
    ILogger<DeleteCourseUseCase> logger) : IDeleteCourseUseCase
{
    public Task<Result<bool>> ExecuteAsync(string courseId, string actorId, CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "courses.delete", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new DeleteCourseRequest(courseId, actorId), cancellationToken));
            var id = UseCaseInput.CourseId(courseId);
            var course = await repository.GetByIdAsync(id, cancellationToken);
            if (course is null)
            {
                logger.CourseNotFoundForDeletion(id);
                return false;
            }

            if (course.DeletedAt is not null)
            {
                return false;
            }

            CourseAuthorization.EnsureAuthor(course, actorId);
            var now = DateTimeOffset.UtcNow;
            var deleted = course with { DeletedAt = now, UpdatedAt = now };
            logger.DeletingCourse(deleted.Id);
            await repository.UpsertAsync(deleted, cancellationToken);
            await eventPublisher.PublishAsync(deleted, actorId, CourseEventType.Deleted, now,
            [
                CourseAuditEntry.Added(nameof(deleted.DeletedAt), deleted.DeletedAt),
                CourseAuditEntry.Updated(nameof(deleted.UpdatedAt), deleted.UpdatedAt)
            ], cancellationToken);
            return true;
        });
}
