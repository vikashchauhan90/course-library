using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Comments.CreateComment;
using CourseLibrary.Application.Operations.Comments;
using CourseLibrary.Application.Operations.Comments.Create;
using CourseLibrary.Models.Course;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Comments.CreateComment;

public sealed class CreateCommentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/comments")
            .WithTags("Comments");

        group.MapPost(
            "/",
            async (
                HttpContext httpContext,
                CreateCommentRequest request,
                IDispatcher dispatcher,
                LinkGenerator linkGenerator,
                ILogger<CreateCommentEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.CreatingComment(request.AuthorId);

                var command = CreateCommentMapper.ToCommand(request);

                var comment = await dispatcher.SendAsync<CreateCommentCommand, CommentResponse>(
                    command,
                    ct);

                logger.CommentCreated(comment.Id);

                var resource = CommentHalHelper.ToResource(linkGenerator, comment);
                return Results.Created(
                    resource.Links.First(x => x.Rel.Equals("self")).Href,
                    resource);
            })
            .WithName("CreateComment")
            .HasApiVersion(1.0);
    }
}
