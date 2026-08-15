using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Create;

public sealed class CourseCreatedEventHandler : IEventNotificationHandler<CourseCreatedEvent>
{
    private readonly ILogger<CourseCreatedEventHandler> _logger;

    public CourseCreatedEventHandler(ILogger<CourseCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(IEventNotification<CourseCreatedEvent> notification, CancellationToken ct)
    {
        var ev = notification.Event;
        // Example: log and perform lightweight async post-processing (e.g., cache warm-up)
        _logger.LogInformation("Course created: {CourseId} by {AuthorId} (title: {Title})", ev.CourseId, ev.AuthorId, ev.Title);
        return Task.CompletedTask;
    }
}
