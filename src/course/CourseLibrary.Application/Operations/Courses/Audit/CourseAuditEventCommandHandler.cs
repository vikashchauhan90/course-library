using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Audit;

public sealed class CourseAuditEventCommandHandler(
    ICourseAuditRepository auditRepository,
    ILogger<CourseAuditEventCommandHandler> logger)
    : IHandler<CourseAuditEventCommand, Unit>
{
    public async Task<Unit> HandleAsync(CourseAuditEventCommand command, CancellationToken ct)
    {
        logger.CreatingCourseAudit(command.Event.CourseId.ToString(), command.Event.EventType);
        await auditRepository.AddAsync(new CourseAuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CourseId = command.Event.CourseId,
            Action = command.Event.EventType,
            EventId = command.Event.EventId,
            ChangedProperties = command.Event.ChangedProperties,
            ActorId = command.Event.ActorId,
            OccurredAt = command.Event.OccurredAt,
            CreatedAt = DateTimeOffset.UtcNow
        }, ct);

        logger.CreatedCourseAudit(command.Event.CourseId.ToString(), command.Event.EventType);
        return Unit.Value;
    }

}
