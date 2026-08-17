using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Authors.GetAuthor;
using CourseLibrary.Application.Operations.Authors.Get;
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
                IDispatcher dispatcher,
                string authorId,
                ILogger<GetAuthorEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.GettingAuthor(authorId);

                var query = GetAuthorMapper.ToQuery(authorId);

                var author = await dispatcher.QueryAsync<GetAuthorQuery, Domain.Entities.Author?>(
                    query,
                    ct);

                if (author is null)
                {
                    logger.AuthorNotFound(authorId);
                    return Results.NotFound();
                }

                logger.AuthorRetrieved(authorId);
                return Results.Ok(author);
            })
            .WithName("GetAuthor")
            .HasApiVersion(1.0);
    }
}
