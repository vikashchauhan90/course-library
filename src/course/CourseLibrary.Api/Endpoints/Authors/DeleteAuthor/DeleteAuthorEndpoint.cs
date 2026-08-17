using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Authors.DeleteAuthor;
using CourseLibrary.Application.Operations.Authors.Delete;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Authors;

public sealed class DeleteAuthorEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/authors")
            .WithTags("Authors");

        group.MapDelete(
            "/{authorId}",
            async (
                HttpContext httpContext,
                IDispatcher dispatcher,
                string authorId,
                ILogger<DeleteAuthorEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.DeletingAuthor(authorId);

                var command = DeleteAuthorMapper.ToCommand(authorId);

                var deleted = await dispatcher.SendAsync<DeleteAuthorCommand, bool>(
                    command,
                    ct);

                if (deleted)
                {
                    logger.AuthorDeleted(authorId);
                    return Results.NoContent();
                }

                logger.AuthorNotFoundForDeletion(authorId);
                return Results.NotFound();
            })
            .WithName("DeleteAuthor")
            .HasApiVersion(1.0);
    }
}
