using CourseLibrary.Application.Abstractions.Repositories;
using CourseLibrary.Application.Operations.Comments;
using CourseLibrary.Application.Operations.Discussions;
using CourseLibrary.Models.Course;
using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Application.Operations.Courses.Get;

public sealed class GetCourseQueryHandler(
        ICourseRepository courseRepository,
        ICommentRepository commentRepository,
        IDiscussionRepository discussionRepository,
        ILogger<GetCourseQueryHandler> logger) 
    : IHandler<GetCourseQuery, CourseResponse?>
{

    public async Task<CourseResponse?> HandleAsync(GetCourseQuery query, CancellationToken ct)
    {
        logger.LogDebug(
            "Retrieving course '{CourseId}' with comments and discussions.",
            query.CourseId);

        var course = await courseRepository.GetByIdAsync(query.CourseId, ct);
        if (course is null)
        {
            logger.LogWarning(
               "Course '{CourseId}' not found.",
               query.CourseId);

            return null;
        }

        var commentsTask = commentRepository.GetByCourseAsync(course.Id, ct);
        var discussionsTask = discussionRepository.GetByCourseAsync(course.Id, ct);
        await Task.WhenAll(commentsTask, discussionsTask);

        return CourseMapper.ToResponse(
            course,
            CommentMapper.ToResponses(await commentsTask),
            DiscussionMapper.ToResponses(await discussionsTask));
    }
}
