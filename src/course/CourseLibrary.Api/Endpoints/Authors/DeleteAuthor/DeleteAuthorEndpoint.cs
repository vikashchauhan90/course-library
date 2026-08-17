using Asp.Versioning;
using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Authors.CreateAuthor;
using CourseLibrary.Api.Endpoints.Authors.UpdateAuthor;
using CourseLibrary.Application.Operations.Authors.Create;
using CourseLibrary.Application.Operations.Authors.Delete;
using CourseLibrary.Application.Operations.Authors.Get;
using CourseLibrary.Application.Operations.Authors.Update;
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

            var deleted = await dispatcher.SendAsync<DeleteAuthorCommand, bool>(
                new DeleteAuthorCommand(authorId),
                ct);

            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteAuthor")
        .WithTags("Authors")
        .HasApiVersion(1.0);
    }
}
