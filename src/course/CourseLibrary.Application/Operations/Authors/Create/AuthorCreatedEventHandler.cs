using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Authors.Create;

public sealed class AuthorCreatedEventHandler : IEventNotificationHandler<AuthorCreatedEvent>
{
    private readonly ILogger<AuthorCreatedEventHandler> _logger;

    public AuthorCreatedEventHandler(ILogger<AuthorCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(IEventNotification<AuthorCreatedEvent> notification, CancellationToken ct)
    {
        var ev = notification.Event;
        _logger.LogInformation("Author created: {AuthorId} {Name}", ev.AuthorId, ev.Name);
        return Task.CompletedTask;
    }
}
