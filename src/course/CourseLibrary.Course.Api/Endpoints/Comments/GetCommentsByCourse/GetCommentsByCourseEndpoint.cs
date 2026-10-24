using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.UseCases.Comments.GetByCourse;

namespace CourseLibrary.Course.Api.Endpoints.Comments.GetCommentsByCourse;

public sealed class GetCommentsByCourseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("comments", "Comments");

        group.MapGet("/course/{courseId}", async (
                string courseId,
                IGetCommentsByCourseUseCase useCase,
                CancellationToken cancellationToken) =>
                (await useCase.ExecuteAsync(courseId, cancellationToken)).ToHttpResult())
            .WithName("GetCommentsByCourse")
            .HasApiVersion(1.0);
    }
}
