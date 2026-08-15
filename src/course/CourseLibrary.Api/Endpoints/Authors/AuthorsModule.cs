using Asp.Versioning;
using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Application.Operations.Authors.Create;
using CourseLibrary.Application.Operations.Authors.Delete;
using CourseLibrary.Application.Operations.Authors.Get;
using CourseLibrary.Application.Operations.Authors.Update;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Authors;

public sealed class AuthorsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/authors")
            .WithTags("Authors");

        group.MapGet("/", async (IDispatcher dispatcher, CancellationToken ct) =>
        {
            var authors = await dispatcher.QueryAsync<GetAuthorsQuery, IReadOnlyList<Domain.Entities.Author>>(
                new GetAuthorsQuery(),
                ct);

            return Results.Ok(authors);
        })
        .WithName("GetAuthors")
        .WithTags("Authors")
        .HasApiVersion(1.0);

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

        group.MapPost("/", async (IDispatcher dispatcher, CreateAuthorRequest request, CancellationToken ct) =>
        {
            var author = await dispatcher.SendAsync<CreateAuthorCommand, Domain.Entities.Author>(
                new CreateAuthorCommand(request.Name, request.Bio, request.Website),
                ct);

            return Results.Created($"/api/v1/authors/{author.Id}", author);
        })
        .WithName("CreateAuthor")
        .WithTags("Authors")
        .HasApiVersion(1.0);

        group.MapPut("/{authorId}", async (IDispatcher dispatcher, string authorId, UpdateAuthorRequest request, CancellationToken ct) =>
        {
            var author = await dispatcher.SendAsync<UpdateAuthorCommand, Domain.Entities.Author>(
                new UpdateAuthorCommand(authorId, request.Name, request.Bio, request.Website),
                ct);

            return Results.Ok(author);
        })
        .WithName("UpdateAuthor")
        .WithTags("Authors")
        .HasApiVersion(1.0);

        group.MapDelete("/{authorId}", async (IDispatcher dispatcher, string authorId, CancellationToken ct) =>
        {
            var deleted = await dispatcher.SendAsync<DeleteAuthorCommand, bool>(
                new DeleteAuthorCommand(authorId),
                ct);

            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteAuthor")
        .WithTags("Authors")
        .HasApiVersion(1.0);
    }

    public sealed record CreateAuthorRequest(string Name, string? Bio, string? Website);
    public sealed record UpdateAuthorRequest(string Name, string? Bio, string? Website);
}
