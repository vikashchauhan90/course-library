using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.ValueObjects;
using CourseLibrary.Models.Course;

namespace CourseLibrary.Application.Operations.Discussions.Create;

public sealed class CreateDiscussionCommandHandler : IHandler<CreateDiscussionCommand, DiscussionResponse>
{
    private readonly IDiscussionRepository _repository;
    private readonly ILogger<CreateDiscussionCommandHandler> _logger;

    public CreateDiscussionCommandHandler(IDiscussionRepository repository, ILogger<CreateDiscussionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<DiscussionResponse> HandleAsync(CreateDiscussionCommand command, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var discussion = new Domain.Entities.Discussion
        {
            Id = DiscussionId.New(),
            CourseId = (CourseId)Guid.Parse(command.CourseId),
            Title = command.Title,
            Description = command.Description,
            CreatedAt = now,
            UpdatedAt = now
        };

        _logger.PersistingDiscussion(discussion.Id.ToString(), discussion.CourseId.ToString());

        await _repository.UpsertAsync(discussion, ct);

        return DiscussionMapper.ToResponse(discussion);
    }
}
