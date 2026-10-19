using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.Operations.Discussions.Update;

namespace CourseLibrary.Course.Api.Endpoints.Discussions.UpdateDiscussion;

public sealed class UpdateDiscussionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("discussions", "Discussions");

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
    }

    public sealed record UpdateDiscussionRequest(string Title, string Description);
}
