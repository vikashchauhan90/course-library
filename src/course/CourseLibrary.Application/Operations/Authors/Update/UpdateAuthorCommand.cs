using MediatorForge.Abstractions;
using CourseLibrary.Application.Operations.Authors;

namespace CourseLibrary.Application.Operations.Authors.Update;

public sealed record UpdateAuthorCommand(string Id, string Name, string? Bio, string? Website) : ICommand<AuthorResponse>;
