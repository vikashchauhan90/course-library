using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.UseCases.Courses.Update;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Platform.AspNetCore.RequestContext;

namespace CourseLibrary.Course.Api.Endpoints.Courses.UpdateCourse;

public sealed class UpdateCourseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("courses", "Courses");

        group.MapPut("/{courseId}", async (
                string courseId,
                UpdateCourseRequest request,
                IRequestContext requestContext,
                IUpdateCourseUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(requestContext.UserId))
                {
                    return Results.Unauthorized();
                }

                return (await useCase.ExecuteAsync(
                    courseId,
                    request,
                    requestContext.UserId,
                    cancellationToken)).ToHttpResult();
            })
            .WithName("UpdateCourse")
            .HasApiVersion(1.0);
    }
}
