using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Authors.Update;

public record UpdateAuthorAuditCommand(
    string AuthorId,
    string Name,
    string Bio,
    string Website,
    string ActorId,
    DateTimeOffset OccurredAt)
    : ICommand<Unit>;