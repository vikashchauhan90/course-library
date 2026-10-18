using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.Operations.Discussions.GetByCourse;

namespace CourseLibrary.Course.Api.Endpoints.Discussions.GetDiscussionsByCourse;

public sealed class GetDiscussionsByCourseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("discussions", "Discussions");

        group.MapGet("/course/{courseId}", async (
                string courseId,
                IGetDiscussionsByCourseOperation operation,
                CancellationToken cancellationToken) =>
                (await operation.ExecuteAsync(courseId, cancellationToken)).ToHttpResult())
            .WithName("GetDiscussionsByCourse")
            .HasApiVersion(1.0);
    }
}
