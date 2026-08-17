using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Application.Operations.Authors.Get;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Authors.GetAuthor;

public sealed class GetAuthorEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/authors")
            .WithTags("Authors");

        group.MapGet("/{authorId}", async (IDispatcher dispatcher, string authorId, CancellationToken ct) =>
        {
            var author = await dispatcher.QueryAsync<GetAuthorQuery, Domain.Entities.Author?>(
                new GetAuthorQuery(authorId),
                ct);

            return author is null ? Results.NotFound() : Results.Ok(author);
        })
        .WithName("GetAuthor")
        .WithTags("Authors")
        .HasApiVersion(1.0);

    }
}
