using MediatorForge.Abstractions;
using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Comments;
using CourseLibrary.Application.Operations.Courses;
using CourseLibrary.Application.Operations.Discussions;

namespace CourseLibrary.Application.Operations.Courses.Get;

public sealed class GetCourseQueryHandler : IHandler<GetCourseQuery, CourseResponse?>
{
    private readonly ICourseRepository _repository;
    private readonly ICommentRepository _commentRepository;
    private readonly IDiscussionRepository _discussionRepository;

    public GetCourseQueryHandler(
        ICourseRepository repository,
        ICommentRepository commentRepository,
        IDiscussionRepository discussionRepository)
    {
        _repository = repository;
        _commentRepository = commentRepository;
        _discussionRepository = discussionRepository;
    }

    public async Task<CourseResponse?> HandleAsync(GetCourseQuery query, CancellationToken ct)
    {
        var course = await _repository.GetByIdAsync(query.CourseId, query.PartitionKey, ct);
        if (course is null)
            return null;

        var commentsTask = _commentRepository.GetByCourseAsync(course.Id, ct);
        var discussionsTask = _discussionRepository.GetByCourseAsync(course.Id, ct);
        await Task.WhenAll(commentsTask, discussionsTask);

        return CourseMapper.ToResponse(
            course,
            CommentMapper.ToResponses(await commentsTask),
            DiscussionMapper.ToResponses(await discussionsTask));
    }
}
