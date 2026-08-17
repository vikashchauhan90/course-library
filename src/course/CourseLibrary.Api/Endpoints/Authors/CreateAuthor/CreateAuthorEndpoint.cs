using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Application.Operations.Authors.Create;
using CourseLibrary.Domain.Entities;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Authors.CreateAuthor;


public sealed class CreateAuthorEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/authors")
            .WithTags("Authors");

        group.MapPost(
                "/",
                async (
                    HttpContext httpContext,
                    CreateAuthorRequest request,
                    IDispatcher dispatcher,
                    ILogger<CreateAuthorEndpoint> logger) =>
                {
                    var ct = httpContext.RequestAborted;

                    logger.CreatingAuthor(request.Name);

                    var command = CreateAuthorMapper.ToCommand(request);

                    var author =
                        await dispatcher.SendAsync<
                            CreateAuthorCommand,
                            Author>(
                            command,
                            ct);

                    logger.AuthorCreated(author.Id);

                    return Results.Created(
                        $"/api/v1/authors/{author.Id}",
                        author);
                })
            .WithName("CreateAuthor")
            .HasApiVersion(1.0);
    }
}