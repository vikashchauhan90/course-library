using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Domain.Abstractions;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Events;
using CourseLibrary.Domain.ValueObjects;
using CourseLibrary.Models.Course;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Create;

public sealed class CreateCourseCommandHandler(
    ICourseRepository repository,
    IRequestContext requestContext,
    ILogger<CreateCourseCommandHandler> logger,
    IEventDispatcher eventDispatcher)
    : IHandler<CreateCourseCommand, CourseResponse>
{
    public async Task<CourseResponse> HandleAsync(CreateCourseCommand command, CancellationToken ct)
    {

        var now = DateTime.UtcNow;
        var course = new Course
        {
            Id = CourseId.New(),
            Title = command.Title,
            Description = command.Description,
            AuthorId = (AuthorId)command.AuthorId,
            AuthorName = command.AuthorName,
            CreatedAt = now,
            UpdatedAt = now
        };

        logger.PersistingCourse(course.Id, course.AuthorId);

        await repository.UpsertAsync(course, ct);

        var courseEvent = new CourseEvent
        {
            EventId = Guid.NewGuid().ToString(),
            CourseId = course.Id,
            ActorId = requestContext.UserId ?? "unknown",
            OccurredAt = now,
            EventType = CourseEventType.Created,
            ChangedProperties = CreateAuditEntries(course),
        };

        // Publish course created event for downstream consumers
        await eventDispatcher.PublishAsync(
            courseEvent,
            ct);

        return CourseMapper.ToResponse(course);
    }

    private static IReadOnlyList<AuditEntry> CreateAuditEntries(Course course) =>
    [
        new() { Action = AuditAction.Add, Name = nameof(course.Title), Value = course.Title, ValueTypeName = course.Title?.GetType().Name },
        new() { Action = AuditAction.Add, Name = nameof(course.Description), Value = course.Description, ValueTypeName = course.Description?.GetType().Name },
        new() { Action = AuditAction.Add, Name = nameof(course.AuthorId), Value = course.AuthorId, ValueTypeName = course.AuthorId.Value.GetType().Name },
        new() { Action = AuditAction.Add, Name = nameof(course.AuthorName), Value = course.AuthorName, ValueTypeName = course.AuthorName?.GetType().Name },
        new() { Action = AuditAction.Add, Name = nameof(course.CreatedAt), Value = course.CreatedAt, ValueTypeName = course.CreatedAt.GetType().Name },
        new() { Action = AuditAction.Add, Name = nameof(course.UpdatedAt), Value = course.UpdatedAt, ValueTypeName = course.UpdatedAt?.GetType().Name }
    ];
}
