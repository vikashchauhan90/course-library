using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Authors.Create;

public sealed class CreateAuthorCommandHandler : IHandler<CreateAuthorCommand, Domain.Entities.Author>
{
    private readonly IAuthorRepository _repository;
    private readonly ILogger<CreateAuthorCommandHandler> _logger;
    private readonly IEventDispatcher _eventDispatcher;

    public CreateAuthorCommandHandler(IAuthorRepository repository, ILogger<CreateAuthorCommandHandler> logger, IEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _logger = logger;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Domain.Entities.Author> HandleAsync(CreateAuthorCommand command, CancellationToken ct)
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

        _logger.LogInformation("Creating author {AuthorId} ({Name})", author.Id, author.Name);
        await _repository.UpsertAsync(author, ct);

        await _eventDispatcher.PublishAsync(new AuthorCreatedEvent(author.Id, author.Name, author.CreatedAt), ct);

        return author;
    }
}
