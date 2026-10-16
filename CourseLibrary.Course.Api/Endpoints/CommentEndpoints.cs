using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.Operations.Comments.Create;
using CourseLibrary.Course.Application.Operations.Comments.Delete;
using CourseLibrary.Course.Application.Operations.Comments.Get;
using CourseLibrary.Course.Application.Operations.Comments.GetByCourse;
using CourseLibrary.Course.Application.Operations.Comments.Update;

namespace CourseLibrary.Course.Api.Endpoints;

public sealed class CommentEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("comments", "Comments");

        group.MapGet("/course/{courseId}", async (
                string courseId,
                IGetCommentsByCourseOperation operation,
                CancellationToken cancellationToken) =>
                (await operation.ExecuteAsync(courseId, cancellationToken)).ToHttpResult())
            .WithName("GetCommentsByCourse")
            .HasApiVersion(1.0);

        group.MapGet("/{commentId}/{courseId}", async (
                string commentId,
                string courseId,
                IGetCommentOperation operation,
                CancellationToken cancellationToken) =>
                (await operation.ExecuteAsync(commentId, courseId, cancellationToken)).ToHttpResult())
            .WithName("GetComment")
            .HasApiVersion(1.0);

        group.MapPost("/", async (
                CreateCommentRequest request,
                ICreateCommentOperation operation,
                CancellationToken cancellationToken) =>
            {
                var result = await operation.ExecuteAsync(
                    request.CourseId,
                    request.AuthorId,
                    request.Content,
                    request.ParentCommentId,
                    cancellationToken);
                return result.ToHttpResult(comment =>
                    Results.CreatedAtRoute("GetComment", new
                    {
                        version = "1",
                        commentId = comment?.Id,
                        courseId = comment?.CourseId
                    }, comment));
            })
            .WithName("CreateComment")
            .HasApiVersion(1.0);

        group.MapPut("/{commentId}/{courseId}", async (
                string commentId,
                string courseId,
                UpdateCommentRequest request,
                IUpdateCommentOperation operation,
                CancellationToken cancellationToken) =>
                (await operation.ExecuteAsync(
                    commentId,
                    courseId,
                    request.Content,
                    cancellationToken)).ToHttpResult())
            .WithName("UpdateComment")
            .HasApiVersion(1.0);

        group.MapDelete("/{commentId}/{courseId}", async (
                string commentId,
                string courseId,
                IDeleteCommentOperation operation,
                CancellationToken cancellationToken) =>
            {
                var result = await operation.ExecuteAsync(commentId, courseId, cancellationToken);
                return result.ToHttpResult(deleted => deleted
                    ? Results.NoContent()
                    : Results.NotFound());
            })
            .WithName("DeleteComment")
            .HasApiVersion(1.0);
    }

    public sealed record CreateCommentRequest(
        string CourseId,
        string AuthorId,
        string Content,
        string? ParentCommentId);

    public sealed record UpdateCommentRequest(string Content);
}
