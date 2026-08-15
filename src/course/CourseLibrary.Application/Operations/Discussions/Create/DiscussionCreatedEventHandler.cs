using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

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
        _logger.LogInformation("Discussion created: {DiscussionId} on course {CourseId} (title {Title})", ev.DiscussionId, ev.CourseId, ev.Title);
        return Task.CompletedTask;
    }
}
