using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Authors.Delete;

public sealed class DeleteAuthorCommandHandler : IHandler<DeleteAuthorCommand, bool>
{
    private readonly IAuthorRepository _repository;
    private readonly IEventDispatcher _eventDispatcher;

    public DeleteAuthorCommandHandler(IAuthorRepository repository, IEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<bool> HandleAsync(DeleteAuthorCommand command, CancellationToken ct)
    {
        await _repository.DeleteAsync(command.AuthorId, ct);
        await _eventDispatcher.PublishAsync(new AuthorDeletedEvent(command.AuthorId), ct);
        return true;
    }
}
