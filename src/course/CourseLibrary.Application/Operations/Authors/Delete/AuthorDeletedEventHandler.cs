using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

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
        _logger.LogInformation("Author deleted: {AuthorId}", ev.AuthorId);
        return Task.CompletedTask;
    }
}
