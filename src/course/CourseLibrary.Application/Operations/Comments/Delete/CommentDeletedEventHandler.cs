using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Operations.Comments;

namespace CourseLibrary.Application.Operations.Comments.Delete;

public sealed class CommentDeletedEventHandler : IEventNotificationHandler<CommentDeletedEvent>
{
    private readonly ILogger<CommentDeletedEventHandler> _logger;

    public CommentDeletedEventHandler(ILogger<CommentDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(IEventNotification<CommentDeletedEvent> notification, CancellationToken ct)
    {
        var ev = notification.Event;
        _logger.CommentDeletedEvent(ev.CommentId);
        return Task.CompletedTask;
    }
}
