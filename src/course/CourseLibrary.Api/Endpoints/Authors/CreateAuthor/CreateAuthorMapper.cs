using CourseLibrary.Application.Operations.Authors.Create;

namespace CourseLibrary.Api.Endpoints.Authors.CreateAuthor;

public static class CreateAuthorMapper
{
    public static CreateAuthorCommand ToCommand(CreateAuthorRequest request)
        => new(request.Name, request.Bio, request.Website);
}
