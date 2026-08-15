using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Discussions.Update;

public sealed class DiscussionUpdatedEventHandler : IEventNotificationHandler<DiscussionUpdatedEvent>
{
    private readonly ILogger<DiscussionUpdatedEventHandler> _logger;

    public DiscussionUpdatedEventHandler(ILogger<DiscussionUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(IEventNotification<DiscussionUpdatedEvent> notification, CancellationToken ct)
    {
        var ev = notification.Event;
        _logger.LogInformation("Discussion updated: {DiscussionId} in course {CourseId} at {UpdatedAt}", ev.DiscussionId, ev.CourseId, ev.UpdatedAt);
        return Task.CompletedTask;
    }
}
