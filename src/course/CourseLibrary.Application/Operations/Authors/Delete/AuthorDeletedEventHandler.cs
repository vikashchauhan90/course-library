using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Authors.Delete;

public sealed class AuthorDeletedEventHandler(
    IEventPublisher eventPublisher,
    ILogger<AuthorDeletedEventHandler> logger) 
    : IEventNotificationHandler<AuthorDeletedEvent>
{

    public async Task HandleAsync(
        IEventNotification<AuthorDeletedEvent> notification,
        CancellationToken ct)
    {
        var ev = notification.Event;
        logger.AuthorDeletedEvent(ev.AuthorId);
        await eventPublisher.PublishAsync(ev, ct);
    }
}
