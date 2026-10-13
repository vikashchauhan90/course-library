using CourseLibrary.Core.Domain.Abstractions.Events;
using CourseLibrary.Course.Domain.Entities;

namespace CourseLibrary.Course.Domain.Events;

public interface IAuditableEntity: IDomainEvent
{
    IReadOnlyList<AuditEntry> ChangedProperties { get; }
}