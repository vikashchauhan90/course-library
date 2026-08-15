using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Comments.Create;

public sealed class CreateCommentCommandHandler : IHandler<CreateCommentCommand, Domain.Entities.Comment>
{
    private readonly ICommentRepository _repository;
    private readonly ILogger<CreateCommentCommandHandler> _logger;
    private readonly IEventDispatcher _eventDispatcher;

    public CreateCommentCommandHandler(ICommentRepository repository, ILogger<CreateCommentCommandHandler> logger, IEventDispatcher eventDispatcher)
    {
        _repository = repository;
        _logger = logger;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Domain.Entities.Comment> HandleAsync(CreateCommentCommand command, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var comment = new Domain.Entities.Comment
        {
            Id = Guid.NewGuid().ToString(),
            CourseId = command.CourseId,
            AuthorId = command.AuthorId,
            Content = command.Content,
            ParentCommentId = command.ParentCommentId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _logger.LogInformation("Creating comment {CommentId} on course {CourseId}", comment.Id, comment.CourseId);

        await _repository.UpsertAsync(comment, ct);

        await _eventDispatcher.PublishAsync(new CommentCreatedEvent(comment.Id, comment.CourseId, comment.AuthorId, comment.CreatedAt), ct);

        return comment;
    }
}
