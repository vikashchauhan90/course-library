using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using CourseLibrary.Models.Course;

namespace CourseLibrary.Application.Operations.Discussions.Update;

public sealed class UpdateDiscussionCommandHandler(IDiscussionRepository repository, ILogger<UpdateDiscussionCommandHandler> logger) : IHandler<UpdateDiscussionCommand, DiscussionResponse>
{
    public async Task<DiscussionResponse> HandleAsync(UpdateDiscussionCommand command, CancellationToken ct)
    {
        logger.UpdatingDiscussion(command.Id);
        var discussionId = (DiscussionId)Guid.Parse(command.Id);
        var courseId = (CourseId)Guid.Parse(command.CourseId);
        var existing = await repository.GetByIdAsync(discussionId, courseId, ct);
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
