using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

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
        _logger.LogInformation("New comment {CommentId} on course {CourseId} by {AuthorId}", ev.CommentId, ev.CourseId, ev.AuthorId);
        return Task.CompletedTask;
    }
}
