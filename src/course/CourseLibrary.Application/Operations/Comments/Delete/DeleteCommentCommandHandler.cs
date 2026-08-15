using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Comments.Delete;

public sealed class DeleteCommentCommandHandler : IHandler<DeleteCommentCommand, bool>
{
    private readonly ICommentRepository _repository;
    private readonly IEventDispatcher _eventDispatcher;

    public DeleteCommentCommandHandler(ICommentRepository repository, IEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<bool> HandleAsync(DeleteCommentCommand command, CancellationToken ct)
    {
        await _repository.DeleteAsync(command.CommentId, command.CourseId, ct);
        await _eventDispatcher.PublishAsync(new CommentDeletedEvent(command.CommentId, command.CourseId), ct);
        return true;
    }
}
