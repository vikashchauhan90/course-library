using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Authors.UpdateAuthor;
using CourseLibrary.Application.Operations.Authors.Update;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Authors;

public sealed class UpdateAuthorEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/authors")
            .WithTags("Authors");

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
    }
}
