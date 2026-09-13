using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Domain.Events;
using CourseLibrary.Domain.ValueObjects;
using CourseLibrary.Models.Course;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using DomainErrors = CourseLibrary.Domain.Exceptions;

namespace CourseLibrary.Application.Operations.Courses.Retire;

public sealed class RetireCourseCommandHandler(
    ICourseRepository repository,
    IRequestContext requestContext,
    IEventDispatcher eventDispatcher,
    ILogger<RetireCourseCommandHandler> logger)
    : IHandler<RetireCourseCommand, CourseResponse?>
{
    public async Task<CourseResponse?> HandleAsync(RetireCourseCommand command, CancellationToken ct)
    {
        var courseId = (CourseId)Guid.Parse(command.CourseId);
        logger.RetiringCourse(courseId);
        var userId = requestContext.UserId
            ?? throw new DomainErrors.UnauthorizedException();


        var course = await repository.GetByIdAsync(courseId, ct);
        if (course is null)
        {
            logger.CourseNotFoundForRetirement(courseId);
            return null;
        }

        if (!string.Equals(course.AuthorId.ToString(), userId, StringComparison.Ordinal))
        {
            logger.LogWarning(
                "User '{UserId}' attempted to delete course '{CourseId}' without proper authorization.",
                userId,
                command.CourseId);
            throw new DomainErrors.UnauthorizedAccessException();


        }

        if (course.RetiredAt is not null)
        {
            logger.LogWarning(
                "User '{UserId}' attempted to retire course '{CourseId}' which is already retired.",
                userId,
                command.CourseId);

            return CourseMapper.ToResponse(course);
        }

        var retiredAt = DateTimeOffset.UtcNow;
        var retired = course with { RetiredAt = retiredAt, UpdatedAt = retiredAt };
        await repository.UpsertAsync(retired, ct);

        var courseEvent = new CourseEvent
        {
            EventId = Guid.NewGuid().ToString(),
            CourseId = course.Id,
            ActorId = requestContext.UserId ?? "unknown",
            OccurredAt = retiredAt,
            EventType = CourseEventType.Retired,
            ChangedProperties = [
                   new() { Action = AuditAction.Add, Name = nameof(retired.RetiredAt), Value = retired.RetiredAt },
                    new() { Action = AuditAction.Updated, Name = nameof(retired.UpdatedAt), Value = retired.UpdatedAt }
               ],
        };
        await eventDispatcher.PublishAsync(
            courseEvent,
            ct);

        return CourseMapper.ToResponse(retired);
    }
}