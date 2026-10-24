using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.UseCases.Courses.Get;

namespace CourseLibrary.Course.Api.Endpoints.Courses.GetCourse;

public sealed class GetCourseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("courses", "Courses");

        group.MapGet("/{courseId}", async (
                string courseId,
                IGetCourseUseCase useCase,
                CancellationToken cancellationToken) =>
                (await useCase.ExecuteAsync(courseId, cancellationToken)).ToHttpResult())
            .WithName("GetCourse")
            .HasApiVersion(1.0);
    }
}
