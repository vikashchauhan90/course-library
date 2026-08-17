using MediatorForge.Abstractions;
using CourseLibrary.Application.Operations.Authors;

namespace CourseLibrary.Application.Operations.Authors.Create;

public sealed record CreateAuthorCommand(string Name, string? Bio, string? Website) : ICommand<AuthorResponse>;
