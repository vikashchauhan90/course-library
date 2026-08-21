using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Discussions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Discussions.Update;

public sealed class UpdateDiscussionCommandHandler(IDiscussionRepository repository, ILogger<UpdateDiscussionCommandHandler> logger, IEventDispatcher eventDispatcher) : IHandler<UpdateDiscussionCommand, DiscussionResponse>
{
    public async Task<DiscussionResponse> HandleAsync(UpdateDiscussionCommand command, CancellationToken ct)
    {
        logger.UpdatingDiscussion(command.Id);
        var existing = await repository.GetByIdAsync(command.Id, command.CourseId, ct);
        if (existing is null)
            throw new KeyNotFoundException($"Discussion '{command.Id}' not found");

        var updated = existing with
        {
            Title = command.Title,
            Description = command.Description,
            UpdatedAt = DateTime.UtcNow
        };

        await repository.UpsertAsync(updated, ct);
        return DiscussionMapper.ToResponse(updated);
    }
}
