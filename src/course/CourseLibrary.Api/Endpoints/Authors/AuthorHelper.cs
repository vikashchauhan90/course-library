using CourseLibrary.Application.Operations.Authors;
using CourseLibrary.Domain.Models;
using Hal.Core;
using Hal.Core.Builders;

namespace CourseLibrary.Api.Endpoints.Authors;

internal static class AuthorHelper
{
    public static IResource<AuthorResponse> GetAuthorResponse(LinkGenerator linkGenerator, AuthorResponse author)
    {
        return new ResourceBuilder<AuthorResponse>(author)
                       .AddLink(
                           "self",
                           linkGenerator.GetPathByName(
                               "GetAuthor",
                               values: new { version = "1", authorId = author.Id })!,
                           HttpVerbs.Get)
                       .AddLink(
                           "collection",
                           linkGenerator.GetPathByName(
                               "GetAuthors",
                               values: new { version = "1" })!,
                           HttpVerbs.Get)
                        .AddLink(
                           "update",
                           linkGenerator.GetPathByName(
                               "UpdateAuthor",
                               values: new { version = "1", authorId = author.Id })!,
                           HttpVerbs.Put)
                         .AddLink(
                           "delete",
                           linkGenerator.GetPathByName(
                               "DeleteAuthor",
                               values: new { version = "1", authorId = author.Id })!,
                           HttpVerbs.Delete)
                       .Build();
    }

    public static IResource<IResource<AuthorResponse>[]> GetAuthorResponse(
    LinkGenerator linkGenerator,
    IEnumerable<AuthorResponse> authors)
    {
        var resources = authors
            .Select(author => GetAuthorResponse(linkGenerator, author))
            .ToArray();

        return new ResourceBuilder<IResource<AuthorResponse>[]>(resources)
            .AddLink(
                "self",
                linkGenerator.GetPathByName(
                    "GetAuthors",
                    values: new { version = "1" })!,
                HttpVerbs.Get)
            .Build();
    }

    public static IResource<PageResult<IResource<AuthorResponse>>> GetAuthorsResponse(
    LinkGenerator linkGenerator,
    PageResult<AuthorResponse> page)
    {
        var response = page.Map(author => GetAuthorResponse(linkGenerator, author));
        return new ResourceBuilder<PageResult<IResource<AuthorResponse>>>(response)
            .AddLink(
                "self",
                linkGenerator.GetPathByName(
                    "GetAuthors",
                    values: new { version = "1" })!,
                HttpVerbs.Get)
            .Build();
    }
}
