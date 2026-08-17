using MediatorForge.Abstractions;
using CourseLibrary.Application.Operations.Authors;

namespace CourseLibrary.Application.Operations.Authors.Get;

public sealed record GetAuthorQuery(string AuthorId) : IQuery<AuthorResponse?>;
