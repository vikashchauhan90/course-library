using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Authors.GetAuthors;
using CourseLibrary.Application.Operations.Authors;
using CourseLibrary.Application.Operations.Authors.Get;
using CourseLibrary.Domain.Models;
using Hal.Core;
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
                LinkGenerator linkGenerator,
                IDispatcher dispatcher,
                ILogger<GetAuthorsEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                int pageSize = httpContext
                .Request.Query.
                TryGetValue("pageSize", out var pageSizeValues) &&
                int.TryParse(pageSizeValues.FirstOrDefault(), out var parsedPageSize)
                    ? parsedPageSize
                    : 10; // Default page size

                string? pageToken = httpContext.Request.Query["pageToken"];
                logger.GettingAllAuthors();

                var query = GetAuthorsMapper.ToQuery(pageSize, pageToken);

                var page = await dispatcher.QueryAsync<GetAuthorsQuery, PageResult<AuthorResponse>>(
                    query,
                    ct);

                logger.AuthorsRetrieved(page.Items.Count);

                IResource<PageResult<IResource<AuthorResponse>>> response =
                    AuthorHelper.GetAuthorsResponse(linkGenerator, page);

                return Results.Ok(response);
            })
            .WithName("GetAuthors")
            .HasApiVersion(1.0);
    }
}
