using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Delete;

public sealed class DeleteCourseAuditHandler(
    ICourseAuditRepository auditRepository,
    ILogger<DeleteCourseAuditHandler> logger)
    : IHandler<DeleteCourseAuditCommand, Unit>
{
    public async Task<Unit> HandleAsync(DeleteCourseAuditCommand command, CancellationToken ct)
    {
        logger.LogInformation("Creating audit entry for CourseId {CourseId}.", command.CourseId);
        await auditRepository.AddAsync(new CourseAuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CourseId = command.CourseId,
            AuthorId = command.AuthorId,
            Action = AuditAction.Deleted,
            Title = command.Title,
            Description = command.Description,
            ActorId = command.ActorId,
            OccurredAt = command.OccurredAt
        }, ct);
        return Unit.Value;
    }
}
