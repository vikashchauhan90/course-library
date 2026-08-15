using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Comments.Update;

public sealed class CommentUpdatedEventHandler : IEventNotificationHandler<CommentUpdatedEvent>
{
    private readonly ILogger<CommentUpdatedEventHandler> _logger;

    public CommentUpdatedEventHandler(ILogger<CommentUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(IEventNotification<CommentUpdatedEvent> notification, CancellationToken ct)
    {
        var ev = notification.Event;
        _logger.LogInformation("Comment updated: {CommentId} on course {CourseId} by {AuthorId} at {UpdatedAt}", ev.CommentId, ev.CourseId, ev.AuthorId, ev.UpdatedAt);
        return Task.CompletedTask;
    }
}
