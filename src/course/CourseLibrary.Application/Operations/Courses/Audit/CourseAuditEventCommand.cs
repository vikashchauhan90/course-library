using CourseLibrary.Domain.Events;
using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Audit;

public sealed record CourseAuditEventCommand(
    CourseEvent Event) : ICommand<Unit>;
