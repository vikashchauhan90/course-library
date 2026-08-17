using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Authors.UpdateAuthor;
using CourseLibrary.Application.Operations.Authors;
using CourseLibrary.Application.Operations.Authors.Update;
using Hal.Core;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Authors.UpdateAuthor;

public sealed class UpdateAuthorEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/authors")
            .WithTags("Authors");

        group.MapPut(
            "/{authorId}",
            async (
                HttpContext httpContext,
                LinkGenerator linkGenerator,
                IDispatcher dispatcher,
                string authorId,
                UpdateAuthorRequest request,
                ILogger<UpdateAuthorEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.UpdatingAuthor(authorId);

                var command = UpdateAuthorMapper.ToCommand(authorId, request);

                var author = await dispatcher.SendAsync<UpdateAuthorCommand, AuthorResponse>(
                    command,
                    ct);

                logger.AuthorUpdated(authorId);

                IResource<AuthorResponse> response =
            AuthorHelper.GetAuthorResponse(linkGenerator, author);

                return Results.Ok(response);
            })
            .WithName("UpdateAuthor")
            .HasApiVersion(1.0);
    }
}
