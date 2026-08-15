using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Discussions.Update;

public sealed class UpdateDiscussionCommandHandler : IHandler<UpdateDiscussionCommand, CourseLibrary.Domain.Entities.Discussion>
{
    private readonly IDiscussionRepository _repository;

    public UpdateDiscussionCommandHandler(IDiscussionRepository repository)
    {
        _repository = repository;
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
        return updated;
    }
}
