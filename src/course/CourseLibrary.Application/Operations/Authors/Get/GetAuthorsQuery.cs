using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Authors.Get;

public sealed record GetAuthorsQuery() : IQuery<IReadOnlyList<Domain.Entities.Author>>;
