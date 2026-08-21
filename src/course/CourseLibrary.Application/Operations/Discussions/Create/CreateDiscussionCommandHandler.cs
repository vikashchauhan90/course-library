using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Discussions;

namespace CourseLibrary.Application.Operations.Discussions.Create;

public sealed class CreateDiscussionCommandHandler : IHandler<CreateDiscussionCommand, DiscussionResponse>
{
    private readonly IDiscussionRepository _repository;
    private readonly ILogger<CreateDiscussionCommandHandler> _logger;
    private readonly IEventDispatcher _eventDispatcher;

    public CreateDiscussionCommandHandler(IDiscussionRepository repository, ILogger<CreateDiscussionCommandHandler> logger, IEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _logger = logger;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<DiscussionResponse> HandleAsync(CreateDiscussionCommand command, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var discussion = new Domain.Entities.Discussion
        {
            Id = Guid.NewGuid().ToString(),
            CourseId = command.CourseId,
            Title = command.Title,
            Description = command.Description,
            CreatedAt = now,
            UpdatedAt = now
        };

        _logger.PersistingDiscussion(discussion.Id, discussion.CourseId);

        await _repository.UpsertAsync(discussion, ct);

        return DiscussionMapper.ToResponse(discussion);
    }
}
