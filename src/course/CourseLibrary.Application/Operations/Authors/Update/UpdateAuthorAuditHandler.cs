using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Authors.Update;

public class UpdateAuthorAuditHandler(
    IAuthorAuditRepository auditRepository,
    ILogger<UpdateAuthorAuditHandler> logger)
    : IHandler<UpdateAuthorAuditCommand, Unit>
{
    public async Task<Unit> HandleAsync(
        UpdateAuthorAuditCommand command,
        CancellationToken ct)
    {
        logger.LogInformation(
            "Creating audit entry for AuthorId {AuthorId}.",
            command.AuthorId);

        await auditRepository.AddAsync(
            new AuthorAuditEntry
            {
                Id = Guid.NewGuid().ToString(),
                AuthorId = command.AuthorId,
                Action = AuditAction.Updated,
                Name = command.Name,
                Bio = command.Bio,
                Website = command.Website,
                ActorId = command.ActorId,
                OccurredAt = command.OccurredAt
            },
            ct);

        return Unit.Value;

    }
}