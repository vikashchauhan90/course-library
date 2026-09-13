using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Domain.ValueObjects;
using CourseLibrary.Models.Course;

namespace CourseLibrary.Application.Operations.Comments.Create;

public sealed class CreateCommentCommandHandler : IHandler<CreateCommentCommand, CommentResponse>
{
    private readonly ICommentRepository _repository;
    private readonly ILogger<CreateCommentCommandHandler> _logger;

    public CreateCommentCommandHandler(ICommentRepository repository, ILogger<CreateCommentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CommentResponse> HandleAsync(CreateCommentCommand command, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var comment = new Domain.Entities.Comment
        {
            Id = CommentId.New(),
            CourseId = (CourseId)Guid.Parse(command.CourseId),
            AuthorId = (AuthorId)command.AuthorId,
            Content = command.Content,
            ParentCommentId = string.IsNullOrWhiteSpace(command.ParentCommentId)
                ? null
                : (CommentId)Guid.Parse(command.ParentCommentId),
            CreatedAt = now,
            UpdatedAt = now
        };

        _logger.PersistingComment(comment.Id.ToString(), comment.CourseId.ToString());

        await _repository.UpsertAsync(comment, ct);

        return CommentMapper.ToResponse(comment);
    }
}
