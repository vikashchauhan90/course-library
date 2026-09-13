using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Events;

public sealed class CourseEventHandler(
    IEventPublisher eventPublisher,
    ILogger<CourseEventHandler> logger) 
    : IEventNotificationHandler<CourseEvent>
{

    public async Task HandleAsync(
        IEventNotification<CourseEvent> notification,
        CancellationToken ct)
    {
        var ev = notification.Event;
        logger.CourseEvent(ev.CourseId.ToString(), (Guid)ev.EventType);
        await eventPublisher.PublishAsync("CourseEvent",ev, ct);
    }
}
