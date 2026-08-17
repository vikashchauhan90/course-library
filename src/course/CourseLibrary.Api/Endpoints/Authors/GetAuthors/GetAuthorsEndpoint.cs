using Carter;
using CourseLibrary.Api.Configuration;
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
                IDispatcher dispatcher,
                CancellationToken ct) =>
        {
            var authors = await dispatcher.QueryAsync<GetAuthorsQuery, IReadOnlyList<Domain.Entities.Author>>(
                new GetAuthorsQuery(),
                ct);

            return Results.Ok(authors);
        })
        .WithName("GetAuthors")
        .WithTags("Authors")
        .HasApiVersion(1.0);
    }
}
