using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Authors.Create;

public sealed class AuthorCreatedEventHandler(ILogger<AuthorCreatedEventHandler> logger) : IEventNotificationHandler<AuthorCreatedEvent>
{

    public Task HandleAsync(IEventNotification<AuthorCreatedEvent> notification, CancellationToken ct)
    {
        var ev = notification.Event;
        logger.AuthorCreated(ev.AuthorId, ev.Name);
        return Task.CompletedTask;
    }
}
