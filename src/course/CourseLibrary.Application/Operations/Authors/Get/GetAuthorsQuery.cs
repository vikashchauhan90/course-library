using MediatorForge.Abstractions;
using CourseLibrary.Application.Operations.Authors;

namespace CourseLibrary.Application.Operations.Authors.Get;

public sealed record GetAuthorsQuery() : IQuery<IReadOnlyList<AuthorResponse>>;
