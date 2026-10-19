using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.Operations.Comments.GetByCourse;

namespace CourseLibrary.Course.Api.Endpoints.Comments.GetCommentsByCourse;

public sealed class GetCommentsByCourseEndpoint : ICarterModule
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
    }
}
