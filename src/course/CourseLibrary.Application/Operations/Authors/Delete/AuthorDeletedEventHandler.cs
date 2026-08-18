using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Operations.Authors;

namespace CourseLibrary.Application.Operations.Authors.Delete;

public sealed class AuthorDeletedEventHandler : IEventNotificationHandler<AuthorDeletedEvent>
{
    private readonly ILogger<AuthorDeletedEventHandler> _logger;

    public AuthorDeletedEventHandler(ILogger<AuthorDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(IEventNotification<AuthorDeletedEvent> notification, CancellationToken ct)
    {
        var ev = notification.Event;
        _logger.AuthorDeletedEvent(ev.AuthorId);
        return Task.CompletedTask;
    }
}
