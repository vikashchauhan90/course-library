using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Operations.Discussions;

namespace CourseLibrary.Application.Operations.Discussions.Delete;

public sealed class DiscussionDeletedEventHandler : IEventNotificationHandler<DiscussionDeletedEvent>
{
    private readonly ILogger<DiscussionDeletedEventHandler> _logger;

    public DiscussionDeletedEventHandler(ILogger<DiscussionDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(IEventNotification<DiscussionDeletedEvent> notification, CancellationToken ct)
    {
        var ev = notification.Event;
        _logger.DiscussionDeletedEvent(ev.DiscussionId);
        return Task.CompletedTask;
    }
}
