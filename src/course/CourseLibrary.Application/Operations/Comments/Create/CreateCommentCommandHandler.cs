using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Comments;

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

        _logger.PersistingComment(comment.Id, comment.CourseId);

        await _repository.UpsertAsync(comment, ct);

        return comment;
    }
}
