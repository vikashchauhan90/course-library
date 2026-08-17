using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Application.Operations.Authors;
using CourseLibrary.Application.Operations.Authors.Create;
using CourseLibrary.Domain.Entities;
using Hal.Core;
using Hal.Core.Builders;
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
                    LinkGenerator linkGenerator,
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
                            AuthorResponse>(
                            command,
                            ct);

                    logger.AuthorCreated(author.Name, author.Id);

                    var authorPath = linkGenerator.GetPathByName(
                        "GetAuthor",
                        values: new { version = "1" ,authorId = author.Id });

                    var response = new ResourceBuilder<AuthorResponse>(author)
                   .AddLink(
                       "self",
                       authorPath!,
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

                    return Results.Created(
                         authorPath,
                        response);
                })
            .WithName("CreateAuthor")
            .HasApiVersion(1.0);
    }
}