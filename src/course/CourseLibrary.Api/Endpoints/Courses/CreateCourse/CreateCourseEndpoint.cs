using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Api.Endpoints.Courses.CreateCourse;
using CourseLibrary.Application.Operations.Courses;
using CourseLibrary.Application.Operations.Courses.Create;
using CourseLibrary.Domain.Entities;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Courses.CreateCourse;

public sealed class CreateCourseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/courses")
            .WithTags("Courses");

        group.MapPost(
            "/",
            async (
                HttpContext httpContext,
                CreateCourseRequest request,
                IDispatcher dispatcher,
                ILogger<CreateCourseEndpoint> logger) =>
            {
                var ct = httpContext.RequestAborted;

                logger.CreatingCourse(request.Title);

                var command = CreateCourseMapper.ToCommand(request);

                var course = await dispatcher.SendAsync<CreateCourseCommand, CourseResponse>(
                    command,
                    ct);

                logger.CourseCreated(course.Id);

                return Results.Created(
                    $"/api/v1/courses/{course.Id}/{course.AuthorId}",
                    course);
            })
            .WithName("CreateCourse")
            .HasApiVersion(1.0);
    }
}
