using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Authors.Update;

public sealed class AuthorUpdatedEventHandler(
    IEventPublisher eventPublisher,
    ILogger<AuthorUpdatedEventHandler> logger) 
    : IEventNotificationHandler<AuthorUpdatedEvent>
{

    public async Task HandleAsync(
        IEventNotification<AuthorUpdatedEvent> notification,
        CancellationToken ct)
    {
        var ev = notification.Event;
        logger.AuthorUpdatedEvent(ev.AuthorId);
        await eventPublisher.PublishAsync(ev, ct);
    }
}
