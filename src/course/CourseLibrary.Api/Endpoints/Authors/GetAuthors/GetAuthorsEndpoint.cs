using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Authors.GetAuthors;
using CourseLibrary.Application.Operations.Authors.Get;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Authors.GetAuthors;

public sealed class GetAuthorsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/authors")
            .WithTags("Authors");

        group.MapGet(
            "/",
            async (
                HttpContext httpContext,
                IDispatcher dispatcher,
                ILogger<GetAuthorsEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.GettingAllAuthors();

                var query = GetAuthorsMapper.ToQuery();

                var authors = await dispatcher.QueryAsync<GetAuthorsQuery, IReadOnlyList<Domain.Entities.Author>>(
                    query,
                    ct);

                logger.AuthorsRetrieved(authors.Count);
                return Results.Ok(authors);
            })
            .WithName("GetAuthors")
            .HasApiVersion(1.0);
    }
}
