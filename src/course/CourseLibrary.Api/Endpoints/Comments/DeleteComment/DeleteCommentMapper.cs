using CourseLibrary.Application.Operations.Comments.Delete;

namespace CourseLibrary.Api.Endpoints.Comments.DeleteComment;

public static class DeleteCommentMapper
{
    public static DeleteCommentCommand ToCommand(string commentId, string courseId)
        => new(commentId, courseId);
}
