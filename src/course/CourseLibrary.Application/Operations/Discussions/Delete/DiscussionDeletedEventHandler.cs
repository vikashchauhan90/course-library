using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

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
        _logger.LogInformation("Discussion deleted: {DiscussionId} from course {CourseId}", ev.DiscussionId, ev.CourseId);
        return Task.CompletedTask;
    }
}
