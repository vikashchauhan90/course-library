using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Abstractions;
using CourseLibrary.Domain.Entities;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Update;

public sealed class UpdateCourseAuditHandler(
    ICourseAuditRepository auditRepository,
    ILogger<UpdateCourseAuditHandler> logger)
    : IHandler<UpdateCourseAuditCommand, Unit>
{
    public async Task<Unit> HandleAsync(UpdateCourseAuditCommand command, CancellationToken ct)
    {
        logger.CreatingCourseUpdateAudit(command.CourseId);
        await auditRepository.AddAsync(new CourseAuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CourseId = command.CourseId,
            Action = AuditAction.Updated,
            EventId = command.EventId,
            ChangedProperties = command.ChangedProperties,
            ActorId = command.ActorId,
            OccurredAt = command.OccurredAt
        }, ct);
        return Unit.Value;
    }
}
