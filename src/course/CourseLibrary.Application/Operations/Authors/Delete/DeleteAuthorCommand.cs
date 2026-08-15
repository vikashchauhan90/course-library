using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Authors.Delete;

public sealed record DeleteAuthorCommand(string AuthorId) : ICommand<bool>;
