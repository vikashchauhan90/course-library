using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Operations.Comments;

namespace CourseLibrary.Application.Operations.Comments.Create;

public sealed class CommentCreatedEventHandler : IEventNotificationHandler<CommentCreatedEvent>
{
    private readonly ILogger<CommentCreatedEventHandler> _logger;

    public CommentCreatedEventHandler(ILogger<CommentCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(IEventNotification<CommentCreatedEvent> notification, CancellationToken ct)
    {
        var ev = notification.Event;
        _logger.CommentCreatedEvent(ev.CommentId);
        return Task.CompletedTask;
    }
}
