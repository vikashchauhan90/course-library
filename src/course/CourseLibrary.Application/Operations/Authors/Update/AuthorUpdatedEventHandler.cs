using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

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
        _logger.LogInformation("Author updated: {AuthorId} ({Name}) at {UpdatedAt}", ev.AuthorId, ev.Name, ev.UpdatedAt);
        return Task.CompletedTask;
    }
}
