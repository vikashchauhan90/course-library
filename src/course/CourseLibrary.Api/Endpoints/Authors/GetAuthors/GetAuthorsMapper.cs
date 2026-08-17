using CourseLibrary.Application.Operations.Authors.Get;

namespace CourseLibrary.Api.Endpoints.Authors.GetAuthors;

public static class GetAuthorsMapper
{
    public static GetAuthorsQuery ToQuery()
        => new();
}
