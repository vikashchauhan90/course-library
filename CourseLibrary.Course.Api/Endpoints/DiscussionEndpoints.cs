using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.Operations.Discussions.Create;
using CourseLibrary.Course.Application.Operations.Discussions.Delete;
using CourseLibrary.Course.Application.Operations.Discussions.Get;
using CourseLibrary.Course.Application.Operations.Discussions.Update;

namespace CourseLibrary.Course.Api.Endpoints;

public sealed class DiscussionEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("discussions", "Discussions");

        group.MapGet("/{discussionId}/{courseId}", async (
                string discussionId,
                string courseId,
                IGetDiscussionOperation operation,
                CancellationToken cancellationToken) =>
                (await operation.ExecuteAsync(discussionId, courseId, cancellationToken)).ToHttpResult())
            .WithName("GetDiscussion")
            .HasApiVersion(1.0);

        group.MapPost("/", async (
                CreateDiscussionRequest request,
                ICreateDiscussionOperation operation,
                CancellationToken cancellationToken) =>
            {
                var result = await operation.ExecuteAsync(
                    request.CourseId,
                    request.Title,
                    request.Description,
                    cancellationToken);
                return result.ToHttpResult(discussion =>
                    Results.CreatedAtRoute("GetDiscussion", new
                    {
                        version = "1",
                        discussionId = discussion?.Id,
                        courseId = discussion?.CourseId
                    }, discussion));
            })
            .WithName("CreateDiscussion")
            .HasApiVersion(1.0);

        group.MapPut("/{discussionId}/{courseId}", async (
                string discussionId,
                string courseId,
                UpdateDiscussionRequest request,
                IUpdateDiscussionOperation operation,
                CancellationToken cancellationToken) =>
                (await operation.ExecuteAsync(
                    discussionId,
                    courseId,
                    request.Title,
                    request.Description,
                    cancellationToken)).ToHttpResult())
            .WithName("UpdateDiscussion")
            .HasApiVersion(1.0);

        group.MapDelete("/{discussionId}/{courseId}", async (
                string discussionId,
                string courseId,
                IDeleteDiscussionOperation operation,
                CancellationToken cancellationToken) =>
            {
                var result = await operation.ExecuteAsync(discussionId, courseId, cancellationToken);
                return result.ToHttpResult(deleted => deleted
                    ? Results.NoContent()
                    : Results.NotFound());
            })
            .WithName("DeleteDiscussion")
            .HasApiVersion(1.0);
    }

    public sealed record CreateDiscussionRequest(string CourseId, string Title, string Description);
    public sealed record UpdateDiscussionRequest(string Title, string Description);
}
