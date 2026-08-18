using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Operations.Authors;

namespace CourseLibrary.Application.Operations.Authors.Create;

public sealed class AuthorCreatedEventHandler(ILogger<AuthorCreatedEventHandler> logger) : IEventNotificationHandler<AuthorCreatedEvent>
{

    public Task HandleAsync(IEventNotification<AuthorCreatedEvent> notification, CancellationToken ct)
    {
        var ev = notification.Event;
        logger.AuthorCreatedEvent(ev.AuthorId);
        return Task.CompletedTask;
    }
}
