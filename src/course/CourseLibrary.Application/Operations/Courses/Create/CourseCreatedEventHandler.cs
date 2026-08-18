using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using CourseLibrary.Application.Operations.Courses;

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
        _logger.CourseCreatedEvent(ev.CourseId);
        return Task.CompletedTask;
    }
}
