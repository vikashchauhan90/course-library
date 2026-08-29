using CourseLibrary.Application.Abstractions.Caching;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.Entities;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Authors.Create;

public class CreateAuthorAuditHandler(
    IAuthorAuditRepository auditRepository,
    ICacheProvider cacheProvider,
    ILogger<CreateAuthorAuditHandler> logger)
    : IHandler<CreateAuthorAuditCommand, Unit>
{
    public async Task<Unit> HandleAsync(
        CreateAuthorAuditCommand command,
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
                Action = AuditAction.Created,
                Name = command.Name,
                Bio = command.Bio,
                Website = command.Website,
                ActorId = command.ActorId,
                OccurredAt = command.OccurredAt
            },
            ct);

        try
        {
            await cacheProvider.RemoveByTagAsync(
                "default",
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to remove cache entries with tag 'default' after creating audit entry for AuthorId {AuthorId}.",
                command.AuthorId);
        }
        return Unit.Value;

    }
}
