using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Comments.CreateComment;
using CourseLibrary.Application.Operations.Comments.Create;
using CourseLibrary.Domain.Entities;
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
                ILogger<CreateCommentEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.CreatingComment(request.AuthorId);

                var command = CreateCommentMapper.ToCommand(request);

                var comment = await dispatcher.SendAsync<CreateCommentCommand, Comment>(
                    command,
                    ct);

                logger.CommentCreated(comment.Id);

                return Results.Created(
                    $"/api/v1/comments/{comment.Id}/{comment.CourseId}",
                    comment);
            })
            .WithName("CreateComment")
            .HasApiVersion(1.0);
    }
}
