using CourseLibrary.Application.Operations.Authors.Get;

namespace CourseLibrary.Api.Endpoints.Authors.GetAuthor;

public static class GetAuthorMapper
{
    public static GetAuthorQuery ToQuery(string authorId)
        => new(authorId);
}
