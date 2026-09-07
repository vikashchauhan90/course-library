using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Models.Course;

namespace CourseLibrary.Application.Operations.Comments.Get;

public sealed class GetCommentQueryHandler : IHandler<GetCommentQuery, CommentResponse?>
{
    private readonly ICommentRepository _repository;

    public GetCommentQueryHandler(ICommentRepository repository)
    {
        _repository = repository;
    }

    public async Task<CommentResponse?> HandleAsync(GetCommentQuery query, CancellationToken ct)
    {
        var comment = await _repository.GetByIdAsync(query.CommentId, query.CourseId, ct);
        return comment is null ? null : CommentMapper.ToResponse(comment);
    }
}
