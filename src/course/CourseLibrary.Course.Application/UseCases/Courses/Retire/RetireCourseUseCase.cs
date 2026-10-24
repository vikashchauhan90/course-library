using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Domain.Events;
using CourseLibrary.Course.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using CourseLibrary.Course.Persistence.Data.Repositories.Abstractions;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Application.UseCases.Courses;

namespace CourseLibrary.Course.Application.UseCases.Courses.Retire;

public interface IRetireCourseUseCase
{
    Task<Result<CourseResponse>> ExecuteAsync(string courseId, string actorId, CancellationToken cancellationToken = default);
}

internal sealed class RetireCourseUseCase(
    ICourseRepository repository,
    ICourseEventPublisher eventPublisher,
    IValidator<RetireCourseRequest> validator,
    ILogger<RetireCourseUseCase> logger) : IRetireCourseUseCase
{
    public Task<Result<CourseResponse>> ExecuteAsync(string courseId, string actorId, CancellationToken cancellationToken = default)
        => CourseApplicationUseCase.ExecuteResultAsync(logger, "courses.retire", async () =>
        {
            ValidationErrors.ThrowIfInvalid(await validator.ValidateAsync(new RetireCourseRequest(courseId, actorId), cancellationToken));
            var id = UseCaseInput.CourseId(courseId);
            var course = await repository.GetByIdAsync(id, cancellationToken)
                ?? throw new DataNotFoundException($"Course '{courseId}' was not found.");
            CourseAuthorization.EnsureAuthor(course, actorId);
            if (course.RetiredAt is not null)
            {
                return CourseMapper.ToResponse(course);
            }

            var now = DateTimeOffset.UtcNow;
            var retired = course with { RetiredAt = now, UpdatedAt = now };
            logger.RetiringCourse(retired.Id);
            await repository.UpsertAsync(retired, cancellationToken);
            await eventPublisher.PublishAsync(retired, actorId, CourseEventType.Retired, now,
            [
                CourseAuditEntry.Added(nameof(retired.RetiredAt), retired.RetiredAt),
                CourseAuditEntry.Updated(nameof(retired.UpdatedAt), retired.UpdatedAt)
            ], cancellationToken);
            return CourseMapper.ToResponse(retired);
        });
}
