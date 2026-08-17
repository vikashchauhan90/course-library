using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Authors.GetAuthor;
using CourseLibrary.Application.Operations.Authors;
using CourseLibrary.Application.Operations.Authors.Get;
using Hal.Core;
using Hal.Core.Builders;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Authors.GetAuthor;

public sealed class GetAuthorEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/authors")
            .WithTags("Authors");

        group.MapGet(
            "/{authorId}",
            async (
                HttpContext httpContext,
                LinkGenerator linkGenerator,
                IDispatcher dispatcher,
                string authorId,
                ILogger<GetAuthorEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.GettingAuthor(authorId);

                var query = GetAuthorMapper.ToQuery(authorId);

                var author = await dispatcher.QueryAsync<GetAuthorQuery, AuthorResponse?>(
                    query,
                    ct);

                if (author is null)
                {
                    logger.AuthorNotFound(authorId);
                    return Results.NotFound();
                }

                logger.AuthorRetrieved(authorId);
                IResource<AuthorResponse> response =
                AuthorHelper.GetAuthorResponse(linkGenerator, author);

                return Results.Ok(response);
            })
            .WithName("GetAuthor")
            .HasApiVersion(1.0);
    }
}
