using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Delete;

public sealed class CourseDeletedEventHandler(
    IEventPublisher eventPublisher,
    ILogger<CourseDeletedEventHandler> logger) 
    : IEventNotificationHandler<CourseDeletedEvent>
{

    public async Task HandleAsync(
        IEventNotification<CourseDeletedEvent> notification,
        CancellationToken ct)
    {
        var ev = notification.Event;
        logger.CourseDeletedEvent(ev.CourseId);
        await eventPublisher.PublishAsync(ev, ct);
    }
}
