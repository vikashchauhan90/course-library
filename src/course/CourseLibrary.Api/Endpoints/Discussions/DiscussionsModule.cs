using Asp.Versioning;
using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Application.Operations.Discussions.Create;
using CourseLibrary.Application.Operations.Discussions.Delete;
using CourseLibrary.Application.Operations.Discussions.Get;
using CourseLibrary.Application.Operations.Discussions.Update;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Discussions;

public sealed class DiscussionsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/discussions")
            .WithTags("Discussions");

        group.MapGet("/{discussionId}/{courseId}", async (IDispatcher dispatcher, string discussionId, string courseId, CancellationToken ct) =>
        {
            var discussion = await dispatcher.QueryAsync<GetDiscussionQuery, Domain.Entities.Discussion?>(
                new GetDiscussionQuery(discussionId, courseId),
                ct);

            return discussion is null ? Results.NotFound() : Results.Ok(discussion);
        })
        .WithName("GetDiscussion")
        .WithTags("Discussions")
        .HasApiVersion(1.0);

        group.MapPost("/", async (IDispatcher dispatcher, CreateDiscussionRequest request, CancellationToken ct) =>
        {
            var discussion = await dispatcher.SendAsync<CreateDiscussionCommand, Domain.Entities.Discussion>(
                new CreateDiscussionCommand(request.CourseId, request.Title, request.Description),
                ct);

            return Results.Created($"/api/v1/discussions/{discussion.Id}/{discussion.CourseId}", discussion);
        })
        .WithName("CreateDiscussion")
        .WithTags("Discussions")
        .HasApiVersion(1.0);

        group.MapPut("/{discussionId}/{courseId}", async (IDispatcher dispatcher, string discussionId, string courseId, UpdateDiscussionRequest request, CancellationToken ct) =>
        {
            var discussion = await dispatcher.SendAsync<UpdateDiscussionCommand, Domain.Entities.Discussion>(
                new UpdateDiscussionCommand(discussionId, courseId, request.Title, request.Description),
                ct);

            return Results.Ok(discussion);
        })
        .WithName("UpdateDiscussion")
        .WithTags("Discussions")
        .HasApiVersion(1.0);

        group.MapDelete("/{discussionId}/{courseId}", async (IDispatcher dispatcher, string discussionId, string courseId, CancellationToken ct) =>
        {
            var deleted = await dispatcher.SendAsync<DeleteDiscussionCommand, bool>(
                new DeleteDiscussionCommand(discussionId, courseId),
                ct);

            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteDiscussion")
        .WithTags("Discussions")
        .HasApiVersion(1.0);
    }

    public sealed record CreateDiscussionRequest(string CourseId, string Title, string Description);
    public sealed record UpdateDiscussionRequest(string Title, string Description);
}
