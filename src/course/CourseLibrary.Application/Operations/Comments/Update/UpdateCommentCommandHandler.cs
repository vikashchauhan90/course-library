using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Comments;
using CourseLibrary.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using CourseLibrary.Models.Course;

namespace CourseLibrary.Application.Operations.Comments.Update;

public sealed class UpdateCommentCommandHandler(ICommentRepository repository, ILogger<UpdateCommentCommandHandler> logger) : IHandler<UpdateCommentCommand, CommentResponse>
{
    public async Task<CommentResponse> HandleAsync(UpdateCommentCommand command, CancellationToken ct)
    {
        logger.UpdatingComment(command.Id);
        var commentId = (CommentId)Guid.Parse(command.Id);
        var courseId = (CourseId)Guid.Parse(command.CourseId);
        var existing = await repository.GetByIdAsync(commentId, courseId, ct);
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
