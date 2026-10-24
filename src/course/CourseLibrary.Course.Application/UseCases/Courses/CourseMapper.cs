using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Domain.Entities;
using CourseEntity = CourseLibrary.Course.Domain.Entities.Course;

namespace CourseLibrary.Course.Application.UseCases.Courses;

internal static class CourseMapper
{
    public static CourseResponse ToResponse(
        CourseEntity course,
        IReadOnlyList<CommentResponse>? comments = null,
        IReadOnlyList<DiscussionResponse>? discussions = null)
        => new(
            course.Id.ToString(),
            course.Title,
            course.Description,
            course.AuthorId.ToString(),
            course.AuthorName,
            course.CreatedAt,
            course.UpdatedAt,
            course.RetiredAt,
            course.DeletedAt,
            comments ?? Array.Empty<CommentResponse>(),
            discussions ?? Array.Empty<DiscussionResponse>());

    public static CommentResponse ToResponse(Comment comment)
        => new(
            comment.Id.ToString(),
            comment.CourseId.ToString(),
            comment.AuthorId.ToString(),
            comment.Content,
            comment.ParentCommentId?.ToString(),
            comment.CreatedAt,
            comment.UpdatedAt);

    public static DiscussionResponse ToResponse(Discussion discussion)
        => new(
            discussion.Id.ToString(),
            discussion.CourseId.ToString(),
            discussion.Title,
            discussion.Description,
            discussion.CreatedAt,
            discussion.UpdatedAt);
}
