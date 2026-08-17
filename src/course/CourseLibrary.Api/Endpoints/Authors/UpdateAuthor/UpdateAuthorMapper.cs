using CourseLibrary.Application.Operations.Authors.Update;

namespace CourseLibrary.Api.Endpoints.Authors.UpdateAuthor;

public static class UpdateAuthorMapper
{
    public static UpdateAuthorCommand ToCommand(string authorId, UpdateAuthorRequest request)
        => new(authorId, request.Name, request.Bio, request.Website);
}
