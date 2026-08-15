using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Authors.Get;

public sealed record GetAuthorQuery(string AuthorId) : IQuery<Domain.Entities.Author?>;
