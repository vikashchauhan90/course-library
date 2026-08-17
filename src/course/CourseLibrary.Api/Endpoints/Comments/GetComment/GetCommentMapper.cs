using CourseLibrary.Application.Operations.Comments.Get;

namespace CourseLibrary.Api.Endpoints.Comments.GetComment;

public static class GetCommentMapper
{
    public static GetCommentQuery ToQuery(string commentId, string courseId)
        => new(commentId, courseId);
}
