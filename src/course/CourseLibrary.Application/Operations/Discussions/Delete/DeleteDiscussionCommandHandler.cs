using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Discussions.Delete;

public sealed class DeleteDiscussionCommandHandler : IHandler<DeleteDiscussionCommand, bool>
{
    private readonly IDiscussionRepository _repository;
    private readonly IEventDispatcher _eventDispatcher;

    public DeleteDiscussionCommandHandler(IDiscussionRepository repository, IEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<bool> HandleAsync(DeleteDiscussionCommand command, CancellationToken ct)
    {
        await _repository.DeleteAsync(command.DiscussionId, command.CourseId, ct);
        await _eventDispatcher.PublishAsync(new DiscussionDeletedEvent(command.DiscussionId, command.CourseId), ct);
        return true;
    }
}
