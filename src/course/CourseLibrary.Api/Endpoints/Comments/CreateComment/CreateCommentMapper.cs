using CourseLibrary.Application.Operations.Comments.Create;

namespace CourseLibrary.Api.Endpoints.Comments.CreateComment;

public static class CreateCommentMapper
{
    public static CreateCommentCommand ToCommand(CreateCommentRequest request)
        => new(request.CourseId, request.AuthorId, request.Content, request.ParentCommentId);
}
