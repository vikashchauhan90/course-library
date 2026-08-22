using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Update;

public sealed class CourseUpdatedEventHandler(
    IEventPublisher eventPublisher,
    ILogger<CourseUpdatedEventHandler> logger) 
    : IEventNotificationHandler<CourseUpdatedEvent>
{

    public async Task HandleAsync(
        IEventNotification<CourseUpdatedEvent> notification,
        CancellationToken ct)
    {
        var ev = notification.Event;
        logger.CourseUpdatedEvent(ev.CourseId);
        await eventPublisher.PublishAsync(ev, ct);
    }
}
