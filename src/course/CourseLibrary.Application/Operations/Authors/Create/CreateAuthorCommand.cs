using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Authors.Create;

public sealed record CreateAuthorCommand(string Name, string? Bio, string? Website) : ICommand<Domain.Entities.Author>;
