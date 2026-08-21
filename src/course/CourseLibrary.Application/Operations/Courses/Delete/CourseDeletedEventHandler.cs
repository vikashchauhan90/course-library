using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Domain.Events;

namespace CourseLibrary.Application.Operations.Courses.Delete;

public sealed class CourseDeletedEventHandler : IEventNotificationHandler<CourseDeletedEvent>
{
    private readonly ILogger<CourseDeletedEventHandler> _logger;

    public CourseDeletedEventHandler(ILogger<CourseDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(IEventNotification<CourseDeletedEvent> notification, CancellationToken ct)
    {
        var ev = notification.Event;
        _logger.CourseDeletedEvent(ev.CourseId);
        return Task.CompletedTask;
    }
}
