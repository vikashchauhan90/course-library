using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Create;

public sealed class CourseCreatedEventHandler(
    IEventPublisher eventPublisher,
    ILogger<CourseCreatedEventHandler> logger) 
    : IEventNotificationHandler<CourseCreatedEvent>
{

    public async Task HandleAsync(
        IEventNotification<CourseCreatedEvent> notification,
        CancellationToken ct)
    {
        var ev = notification.Event;
        logger.CourseCreatedEvent(ev.CourseId);
        await eventPublisher.PublishAsync(ev, ct);
    }
}
