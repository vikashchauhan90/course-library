using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Domain.Abstractions;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Events;
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
            Id = Guid.NewGuid().ToString(),
            Title = command.Title,
            Description = command.Description,
            AuthorId = command.AuthorId,
            AuthorName = command.AuthorName,
            CreatedAt = now,
            UpdatedAt = now
        };

        logger.PersistingCourse(course.Id, course.AuthorId);

        await repository.UpsertAsync(course, ct);

        // Publish course created event for downstream consumers
        await eventDispatcher.PublishAsync(
            new CourseCreatedEvent(
                course.Id,
                Guid.NewGuid().ToString(),
                requestContext.UserId ?? "unknown",
                course.CreatedAt,
                CreateAuditEntries(course)),
            ct);

        return CourseMapper.ToResponse(course);
    }

    private static IReadOnlyList<AuditEntry> CreateAuditEntries(Course course) =>
    [
        new() { Action = AuditAction.Created, Name = nameof(course.Title), Value = course.Title },
        new() { Action = AuditAction.Created, Name = nameof(course.Description), Value = course.Description },
        new() { Action = AuditAction.Created, Name = nameof(course.AuthorId), Value = course.AuthorId },
        new() { Action = AuditAction.Created, Name = nameof(course.AuthorName), Value = course.AuthorName },
        new() { Action = AuditAction.Created, Name = nameof(course.CreatedAt), Value = course.CreatedAt },
        new() { Action = AuditAction.Created, Name = nameof(course.UpdatedAt), Value = course.UpdatedAt }
    ];
}
