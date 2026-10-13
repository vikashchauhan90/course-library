using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.Events;
using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Course.Persistence.Abstractions.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.Operations.Courses.Retire;

public interface IRetireCourseOperation
{
    Task<Result<CourseResponse>> ExecuteAsync(string courseId, string actorId, CancellationToken cancellationToken = default);
}

internal sealed class RetireCourseOperation(
    ICourseRepository repository,
    CourseEventPublisher eventPublisher,
    IValidator<RetireCourseRequest> validator,
    ILogger<RetireCourseOperation> logger) : IRetireCourseOperation
{
    public Task<Result<CourseResponse>> ExecuteAsync(string courseId, string actorId, CancellationToken cancellationToken = default)
        => CourseApplicationOperation.ExecuteResultAsync(logger, "courses.retire", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new RetireCourseRequest(courseId, actorId), cancellationToken));
            var id = OperationInput.CourseId(courseId);
            var course = await repository.GetByIdAsync(id, cancellationToken)
                ?? throw new DataNotFoundException($"Course '{courseId}' was not found.");
            CourseAuthorization.EnsureAuthor(course, actorId);
            if (course.RetiredAt is not null)
            {
                return CourseMapper.ToResponse(course);
            }

            var now = DateTimeOffset.UtcNow;
            var retired = course with { RetiredAt = now, UpdatedAt = now };
            await repository.UpsertAsync(retired, cancellationToken);
            await eventPublisher.PublishAsync(retired, actorId, CourseEventType.Retired, now,
            [
                CourseAuditEntry.Added(nameof(retired.RetiredAt), retired.RetiredAt),
                CourseAuditEntry.Updated(nameof(retired.UpdatedAt), retired.UpdatedAt)
            ], cancellationToken);
            return CourseMapper.ToResponse(retired);
        });
}
