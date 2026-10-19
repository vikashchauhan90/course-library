using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.Operations.Discussions.Get;

namespace CourseLibrary.Course.Api.Endpoints.Discussions.GetDiscussion;

public sealed class GetDiscussionEndpoint : ICarterModule
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
    }
}
