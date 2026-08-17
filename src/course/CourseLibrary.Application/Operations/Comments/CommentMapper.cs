using CourseLibrary.Domain.Entities;

namespace CourseLibrary.Application.Operations.Comments;

/// <summary>
/// Mapper for Comment domain entity to response models.
/// </summary>
public static class CommentMapper
{
    public static CommentResponse ToResponse(Comment comment)
        => new(
            comment.Id,
            comment.CourseId,
            comment.AuthorId,
            comment.Content,
            comment.ParentCommentId,
            comment.CreatedAt,
            comment.UpdatedAt);


    public static IReadOnlyList<CommentResponse> ToResponses(IReadOnlyList<Comment> comments)
        => comments.Select(ToResponse).ToList().AsReadOnly();
}
