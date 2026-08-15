using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Authors.Update;

public sealed record UpdateAuthorCommand(string Id, string Name, string? Bio, string? Website) : ICommand<CourseLibrary.Domain.Entities.Author>;
