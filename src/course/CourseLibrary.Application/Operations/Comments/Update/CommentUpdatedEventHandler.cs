using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Operations.Comments;

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
        _logger.CommentUpdatedEvent(ev.CommentId);
        return Task.CompletedTask;
    }
}
