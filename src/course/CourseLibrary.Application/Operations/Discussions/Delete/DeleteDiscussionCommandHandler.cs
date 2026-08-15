using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Discussions.Delete;

public sealed class DeleteDiscussionCommandHandler : IHandler<DeleteDiscussionCommand, bool>
{
    private readonly IDiscussionRepository _repository;

    public DeleteDiscussionCommandHandler(IDiscussionRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> HandleAsync(DeleteDiscussionCommand command, CancellationToken ct)
    {
        await _repository.DeleteAsync(command.DiscussionId, command.CourseId, ct);
        return true;
    }
}
