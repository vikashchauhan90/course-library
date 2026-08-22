using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Authors.Create;

public sealed class AuthorCreatedEventHandler(
    IEventPublisher eventPublisher,
    ILogger<AuthorCreatedEventHandler> logger)
    : IEventNotificationHandler<AuthorCreatedEvent>
{

    public async Task HandleAsync(
        IEventNotification<AuthorCreatedEvent> notification,
        CancellationToken ct)
    {
        var ev = notification.Event;
        logger.AuthorCreatedEvent(ev.AuthorId);
        await eventPublisher.PublishAsync(ev, ct);
    }
}
