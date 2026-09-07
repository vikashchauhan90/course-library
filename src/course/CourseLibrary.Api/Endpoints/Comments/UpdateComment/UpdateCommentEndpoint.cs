using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Comments.UpdateComment;
using CourseLibrary.Application.Operations.Comments;
using CourseLibrary.Application.Operations.Comments.Update;
using CourseLibrary.Models.Course;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Comments.UpdateComment;

public sealed class UpdateCommentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/comments")
            .WithTags("Comments");

        group.MapPut(
            "/{commentId}/{courseId}",
            async (
                HttpContext httpContext,
                IDispatcher dispatcher,
                string commentId,
                string courseId,
                UpdateCommentRequest request,
                LinkGenerator linkGenerator,
                ILogger<UpdateCommentEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.UpdatingComment(commentId);

                var command = UpdateCommentMapper.ToCommand(commentId, courseId, request);

                var comment = await dispatcher.SendAsync<UpdateCommentCommand, CommentResponse>(
                    command,
                    ct);

                logger.CommentUpdated(commentId);
                return Results.Ok(CommentHalHelper.ToResource(
                    linkGenerator,
                    comment));
            })
            .WithName("UpdateComment")
            .HasApiVersion(1.0);
    }
}
