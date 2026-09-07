using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Comments;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Comments.Update;

public sealed class UpdateCommentCommandHandler(ICommentRepository repository, ILogger<UpdateCommentCommandHandler> logger) : IHandler<UpdateCommentCommand, CommentResponse>
{
    public async Task<CommentResponse> HandleAsync(UpdateCommentCommand command, CancellationToken ct)
    {
        logger.UpdatingComment(command.Id);
        var existing = await repository.GetByIdAsync(command.Id, command.CourseId, ct);
        if (existing is null)
            throw new KeyNotFoundException($"Comment '{command.Id}' not found");

        var updated = existing with
        {
            Content = command.Content,
            UpdatedAt = DateTime.UtcNow
        };

        await repository.UpsertAsync(updated, ct);
        return CommentMapper.ToResponse(updated);
    }
}
