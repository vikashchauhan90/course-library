using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

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
        _logger.LogInformation("Comment deleted: {CommentId} from course {CourseId}", ev.CommentId, ev.CourseId);
        return Task.CompletedTask;
    }
}
