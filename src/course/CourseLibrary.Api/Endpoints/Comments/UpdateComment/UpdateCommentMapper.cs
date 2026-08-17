using CourseLibrary.Application.Operations.Comments.Update;

namespace CourseLibrary.Api.Endpoints.Comments.UpdateComment;

public static class UpdateCommentMapper
{
    public static UpdateCommentCommand ToCommand(string commentId, string courseId, UpdateCommentRequest request)
        => new(commentId, courseId, request.Content);
}
