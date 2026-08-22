using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Domain.Entities;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Authors.Create;

public sealed class CreateAuthorCommandHandler(
    IAuthorRepository repository,
    IAuthorAuditRepository auditRepository,
    IRequestContext requestContext,
    ILogger<CreateAuthorCommandHandler> logger,
    IEventDispatcher eventDispatcher) : IHandler<CreateAuthorCommand, AuthorResponse>
{

    public async Task<AuthorResponse> HandleAsync(CreateAuthorCommand command, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var author = new Domain.Entities.Author
        {
            Id = Guid.NewGuid().ToString(),
            Name = command.Name,
            Bio = command.Bio,
            Website = command.Website,
            CreatedAt = now,
            UpdatedAt = now
        };

        logger.CreatingAuthor(author.Id, author.Name);
        await repository.UpsertAsync(author, ct);
        await auditRepository.AddAsync(new AuthorAuditEntry
        {
            Id = Guid.NewGuid().ToString(), AuthorId = author.Id, Action = AuditAction.Created,
            Name = author.Name, Bio = author.Bio, Website = author.Website, OccurredAt = author.CreatedAt,
            ActorId = requestContext.UserId, CorrelationId = requestContext.CorrelationId
        }, ct);

        await eventDispatcher.PublishAsync(
            new AuthorCreatedEvent(
                author.Id,
                author.Name,
                Guid.NewGuid().ToString(),
                author.CreatedAt),
            ct);

        return AuthorMapper.ToResponse(author);
    }
}
