using CourseLibrary.Domain.Models;
using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Authors.Get;

public sealed record GetAuthorsQuery(int PageSize, string? PageToken) : IQuery<PageResult<AuthorResponse>>;
