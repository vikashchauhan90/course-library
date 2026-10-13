using CourseLibrary.Course.Application.Core;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.Events;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.Operations.Courses.Delete;

public interface IDeleteCourseOperation
{
    Task<Result<bool>> ExecuteAsync(string courseId, string actorId, CancellationToken cancellationToken = default);
}

internal sealed class DeleteCourseOperation(
    ICourseRepository repository,
    CourseEventPublisher eventPublisher,
    IValidator<DeleteCourseRequest> validator,
    ILogger<DeleteCourseOperation> logger) : IDeleteCourseOperation
{
    public Task<Result<bool>> ExecuteAsync(string courseId, string actorId, CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "courses.delete", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new DeleteCourseRequest(courseId, actorId), cancellationToken));
            var id = OperationInput.CourseId(courseId);
            var course = await repository.GetByIdAsync(id, cancellationToken);
            if (course is null || course.DeletedAt is not null)
            {
                return false;
            }

            CourseAuthorization.EnsureAuthor(course, actorId);
            var now = DateTimeOffset.UtcNow;
            var deleted = course with { DeletedAt = now, UpdatedAt = now };
            await repository.UpsertAsync(deleted, cancellationToken);
            await eventPublisher.PublishAsync(deleted, actorId, CourseEventType.Deleted, now,
            [
                CourseAuditEntry.Added(nameof(deleted.DeletedAt), deleted.DeletedAt),
                CourseAuditEntry.Updated(nameof(deleted.UpdatedAt), deleted.UpdatedAt)
            ], cancellationToken);
            return true;
        });
}
