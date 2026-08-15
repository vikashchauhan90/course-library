using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Discussions.Update;

public sealed class UpdateDiscussionCommandHandler : IHandler<UpdateDiscussionCommand, CourseLibrary.Domain.Entities.Discussion>
{
    private readonly IDiscussionRepository _repository;
    private readonly IEventDispatcher _eventDispatcher;

    public UpdateDiscussionCommandHandler(IDiscussionRepository repository, IEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<CourseLibrary.Domain.Entities.Discussion> HandleAsync(UpdateDiscussionCommand command, CancellationToken ct)
    {
        var existing = await _repository.GetByIdAsync(command.Id, command.CourseId, ct);
        if (existing is null)
            throw new KeyNotFoundException($"Discussion '{command.Id}' not found");

        var updated = existing with
        {
            Title = command.Title,
            Description = command.Description,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.UpsertAsync(updated, ct);
        await _eventDispatcher.PublishAsync(new DiscussionUpdatedEvent(updated.Id, updated.CourseId, updated.Title, updated.UpdatedAt), ct);
        return updated;
    }
}
