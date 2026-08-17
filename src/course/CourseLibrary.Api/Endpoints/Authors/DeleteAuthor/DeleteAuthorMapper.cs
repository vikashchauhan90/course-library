using CourseLibrary.Application.Operations.Authors.Delete;

namespace CourseLibrary.Api.Endpoints.Authors.DeleteAuthor;

public static class DeleteAuthorMapper
{
    public static DeleteAuthorCommand ToCommand(string authorId)
        => new(authorId);
}
