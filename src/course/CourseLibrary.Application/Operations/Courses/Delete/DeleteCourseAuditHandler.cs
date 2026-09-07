using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Abstractions;
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
        logger.CreatingCourseDeleteAudit(command.CourseId);
        await auditRepository.AddAsync(new CourseAuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CourseId = command.CourseId,
            AuthorId = command.AuthorId,
            AuthorName = command.AuthorName,
            Action = AuditAction.Deleted,
            Title = command.Title,
            Description = command.Description,
            EventId = command.EventId,
            CreatedAt = command.CreatedAt,
            UpdatedAt = command.UpdatedAt,
            RetiredAt = command.RetiredAt,
            DeletedAt = command.DeletedAt,
            ChangedProperties = command.ChangedProperties,
            ActorId = command.ActorId,
            OccurredAt = command.OccurredAt
        }, ct);
        return Unit.Value;
    }
}
