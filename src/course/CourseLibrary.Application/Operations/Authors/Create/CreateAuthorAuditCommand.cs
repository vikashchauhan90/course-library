using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Authors.Create;

public record CreateAuthorAuditCommand(
    string AuthorId,
    string Name,
    DateTimeOffset OccurredAt)
    : ICommand<Unit>;

