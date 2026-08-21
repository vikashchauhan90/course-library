using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Domain.Events;

namespace CourseLibrary.Application.Operations.Authors.Update;

public sealed class AuthorUpdatedEventHandler : IEventNotificationHandler<AuthorUpdatedEvent>
{
    private readonly ILogger<AuthorUpdatedEventHandler> _logger;

    public AuthorUpdatedEventHandler(ILogger<AuthorUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(IEventNotification<AuthorUpdatedEvent> notification, CancellationToken ct)
    {
        var ev = notification.Event;
        _logger.AuthorUpdatedEvent(ev.AuthorId);
        return Task.CompletedTask;
    }
}
