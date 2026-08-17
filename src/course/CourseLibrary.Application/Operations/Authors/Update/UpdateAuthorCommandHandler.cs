using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Authors;

namespace CourseLibrary.Application.Operations.Authors.Update;

public sealed class UpdateAuthorCommandHandler : IHandler<UpdateAuthorCommand, AuthorResponse>
{
    private readonly IAuthorRepository _repository;
    private readonly IEventDispatcher _eventDispatcher;

    public UpdateAuthorCommandHandler(IAuthorRepository repository, IEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<AuthorResponse> HandleAsync(UpdateAuthorCommand command, CancellationToken ct)
    {
        var existing = await _repository.GetByIdAsync(command.Id, ct);
        if (existing is null)
            throw new KeyNotFoundException($"Author '{command.Id}' not found");

        var updated = existing with
        {
            Name = command.Name,
            Bio = command.Bio,
            Website = command.Website,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.UpsertAsync(updated, ct);
        await _eventDispatcher.PublishAsync(new AuthorUpdatedEvent(updated.Id, updated.Name, updated.UpdatedAt), ct);
        return AuthorMapper.ToResponse(updated);
    }
}
