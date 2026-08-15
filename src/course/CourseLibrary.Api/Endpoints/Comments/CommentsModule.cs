using Asp.Versioning;
using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Application.Operations.Comments.Create;
using CourseLibrary.Application.Operations.Comments.Delete;
using CourseLibrary.Application.Operations.Comments.Get;
using CourseLibrary.Application.Operations.Comments.Update;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Comments;

public sealed class CommentsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/comments")
            .WithTags("Comments");

        group.MapGet("/{commentId}/{courseId}", async (IDispatcher dispatcher, string commentId, string courseId, CancellationToken ct) =>
        {
            var comment = await dispatcher.QueryAsync<GetCommentQuery, Domain.Entities.Comment?>(
                new GetCommentQuery(commentId, courseId),
                ct);

            return comment is null ? Results.NotFound() : Results.Ok(comment);
        })
        .WithName("GetComment")
        .WithTags("Comments")
        .HasApiVersion(1.0);

        group.MapPost("/", async (IDispatcher dispatcher, CreateCommentRequest request, CancellationToken ct) =>
        {
            var comment = await dispatcher.SendAsync<CreateCommentCommand, Domain.Entities.Comment>(
                new CreateCommentCommand(request.CourseId, request.AuthorId, request.Content, request.ParentCommentId),
                ct);

            return Results.Created($"/api/v1/comments/{comment.Id}/{comment.CourseId}", comment);
        })
        .WithName("CreateComment")
        .WithTags("Comments")
        .HasApiVersion(1.0);

        group.MapPut("/{commentId}/{courseId}", async (IDispatcher dispatcher, string commentId, string courseId, UpdateCommentRequest request, CancellationToken ct) =>
        {
            var comment = await dispatcher.SendAsync<UpdateCommentCommand, Domain.Entities.Comment>(
                new UpdateCommentCommand(commentId, courseId, request.Content),
                ct);

            return Results.Ok(comment);
        })
        .WithName("UpdateComment")
        .WithTags("Comments")
        .HasApiVersion(1.0);

        group.MapDelete("/{commentId}/{courseId}", async (IDispatcher dispatcher, string commentId, string courseId, CancellationToken ct) =>
        {
            var deleted = await dispatcher.SendAsync<DeleteCommentCommand, bool>(
                new DeleteCommentCommand(commentId, courseId),
                ct);

            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteComment")
        .WithTags("Comments")
        .HasApiVersion(1.0);
    }

    public sealed record CreateCommentRequest(string CourseId, string AuthorId, string Content, string? ParentCommentId);
    public sealed record UpdateCommentRequest(string Content);
}
