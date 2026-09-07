using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Abstractions;
using CourseLibrary.Domain.Entities;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Create;

public sealed class CreateCourseAuditHandler(
    ICourseAuditRepository auditRepository,
    ILogger<CreateCourseAuditHandler> logger)
    : IHandler<CreateCourseAuditCommand, Unit>
{
    public async Task<Unit> HandleAsync(CreateCourseAuditCommand command, CancellationToken ct)
    {
        logger.CreatingCourseAudit(command.CourseId);
        await auditRepository.AddAsync(new CourseAuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CourseId = command.CourseId,
            Action = AuditAction.Created,
            EventId = command.EventId,
            ChangedProperties = command.ChangedProperties,
            ActorId = command.ActorId,
            OccurredAt = command.OccurredAt
        }, ct);
        return Unit.Value;
    }
}
