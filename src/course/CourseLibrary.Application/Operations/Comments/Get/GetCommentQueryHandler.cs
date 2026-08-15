using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;

namespace CourseLibrary.Application.Operations.Comments.Get;

public sealed class GetCommentQueryHandler : IHandler<GetCommentQuery, CourseLibrary.Domain.Entities.Comment?>
{
    private readonly ICommentRepository _repository;

    public GetCommentQueryHandler(ICommentRepository repository)
    {
        _repository = repository;
    }

    public Task<CourseLibrary.Domain.Entities.Comment?> HandleAsync(GetCommentQuery query, CancellationToken ct)
        => _repository.GetByIdAsync(query.CommentId, query.CourseId, ct);
}
