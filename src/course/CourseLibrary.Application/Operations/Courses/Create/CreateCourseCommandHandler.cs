using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Application.Operations.Courses;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Create;

public sealed class CreateCourseCommandHandler(ICourseRepository repository, ICourseAuditRepository auditRepository, IRequestContext requestContext, ILogger<CreateCourseCommandHandler> logger, IEventDispatcher eventDispatcher) : IHandler<CreateCourseCommand, CourseResponse>
{
    public async Task<CourseResponse> HandleAsync(CreateCourseCommand command, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var course = new Domain.Entities.Course
        {
            Id = Guid.NewGuid().ToString(),
            Title = command.Title,
            Description = command.Description,
            AuthorId = command.AuthorId,
            CreatedAt = now,
            UpdatedAt = now
        };

        logger.PersistingCourse(course.Id, course.AuthorId);

        await repository.UpsertAsync(course, ct);
        await auditRepository.AddAsync(new CourseAuditEntry { Id = Guid.NewGuid().ToString(), CourseId = course.Id, Action = AuditAction.Created, AuthorId = course.AuthorId, Title = course.Title, Description = course.Description, OccurredAt = course.CreatedAt, ActorId = requestContext.UserId, CorrelationId = requestContext.CorrelationId }, ct);

        // Publish course created event for downstream consumers
        await eventDispatcher.PublishAsync(new CourseCreatedEvent(course.Id, course.AuthorId, course.Title, course.CreatedAt), ct);

        return CourseMapper.ToResponse(course);
    }
}
