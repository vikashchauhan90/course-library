using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Authors.Create;

public record CreateAuthorAuditCommand(
    string AuthorId,
    string Name,
    string Bio,
    string Website,
    string ActorId,
    DateTimeOffset OccurredAt)
    : ICommand<Unit>;

