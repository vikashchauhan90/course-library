using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Comments.Update;

public sealed class UpdateCommentCommandHandler : IHandler<UpdateCommentCommand, CourseLibrary.Domain.Entities.Comment>
{
    private readonly ICommentRepository _repository;
    private readonly IEventDispatcher _eventDispatcher;

    public UpdateCommentCommandHandler(ICommentRepository repository, IEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<CourseLibrary.Domain.Entities.Comment> HandleAsync(UpdateCommentCommand command, CancellationToken ct)
    {
        var existing = await _repository.GetByIdAsync(command.Id, command.CourseId, ct);
        if (existing is null)
            throw new KeyNotFoundException($"Comment '{command.Id}' not found");

        var updated = existing with
        {
            Content = command.Content,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.UpsertAsync(updated, ct);
        await _eventDispatcher.PublishAsync(new CommentUpdatedEvent(updated.Id, updated.CourseId, updated.AuthorId, updated.UpdatedAt), ct);
        return updated;
    }
}
