using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Operations.Discussions;

namespace CourseLibrary.Application.Operations.Discussions.Create;

public sealed class DiscussionCreatedEventHandler : IEventNotificationHandler<DiscussionCreatedEvent>
{
    private readonly ILogger<DiscussionCreatedEventHandler> _logger;

    public DiscussionCreatedEventHandler(ILogger<DiscussionCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(IEventNotification<DiscussionCreatedEvent> notification, CancellationToken ct)
    {
        var ev = notification.Event;
        _logger.DiscussionCreatedEvent(ev.DiscussionId);
        return Task.CompletedTask;
    }
}
