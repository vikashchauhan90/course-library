using CourseLibrary.Domain.Entities;
using CourseLibrary.Models.Course;

namespace CourseLibrary.Application.Operations.Comments;

/// <summary>
/// Mapper for Comment domain entity to response models.
/// </summary>
public static class CommentMapper
{
    public static CommentResponse ToResponse(Comment comment)
        => new(
            comment.Id.ToString(),
            comment.CourseId.ToString(),
            comment.AuthorId.ToString(),
            comment.Content,
            comment.ParentCommentId?.ToString(),
            comment.CreatedAt,
            comment.UpdatedAt);


    public static IReadOnlyList<CommentResponse> ToResponses(IReadOnlyList<Comment> comments)
        => comments.Select(ToResponse).ToList().AsReadOnly();
}
