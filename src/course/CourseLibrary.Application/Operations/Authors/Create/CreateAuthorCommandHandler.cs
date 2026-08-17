using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Authors.Create;

public sealed class CreateAuthorCommandHandler(
    IAuthorRepository repository,
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

        await eventDispatcher.PublishAsync(new AuthorCreatedEvent(author.Id, author.Name, author.CreatedAt), ct);

        return AuthorMapper.ToResponse(author);
    }
}
