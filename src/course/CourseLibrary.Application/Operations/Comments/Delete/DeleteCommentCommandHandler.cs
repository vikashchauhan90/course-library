using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Comments.Delete;

public sealed class DeleteCommentCommandHandler : IHandler<DeleteCommentCommand, bool>
{
    private readonly ICommentRepository _repository;

    public DeleteCommentCommandHandler(ICommentRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> HandleAsync(DeleteCommentCommand command, CancellationToken ct)
    {
        await _repository.DeleteAsync(command.CommentId, command.CourseId, ct);
        return true;
    }
}
