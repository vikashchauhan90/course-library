using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Comments.DeleteComment;
using CourseLibrary.Application.Operations.Comments.Delete;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Comments.DeleteComment;

public sealed class DeleteCommentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/comments")
            .WithTags("Comments");

        group.MapDelete(
            "/{commentId}/{courseId}",
            async (
                HttpContext httpContext,
                IDispatcher dispatcher,
                string commentId,
                string courseId,
                ILogger<DeleteCommentEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.DeletingComment(commentId);

                var command = DeleteCommentMapper.ToCommand(commentId, courseId);

                var deleted = await dispatcher.SendAsync<DeleteCommentCommand, bool>(
                    command,
                    ct);

                if (deleted)
                {
                    logger.CommentDeleted(commentId);
                    return Results.NoContent();
                }

                logger.CommentNotFoundForDeletion(commentId);
                return Results.NotFound();
            })
            .WithName("DeleteComment")
            .HasApiVersion(1.0);
    }
}
