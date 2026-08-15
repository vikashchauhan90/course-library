using Asp.Versioning;
using Carter;
using CourseLibrary.Api.Configuration;
using CourseLibrary.Application.Operations.Courses.Create;
using CourseLibrary.Application.Operations.Courses.Delete;
using CourseLibrary.Application.Operations.Courses.Get;
using CourseLibrary.Application.Operations.Courses.Update;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Endpoints.Courses;

public sealed class CoursesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/courses")
            .WithTags("Courses");

        group.MapGet("/{courseId}/{partitionKey}", async (IDispatcher dispatcher, string courseId, string partitionKey, CancellationToken ct) =>
        {
            var course = await dispatcher.QueryAsync<GetCourseQuery, Domain.Entities.Course?>(
                new GetCourseQuery(courseId, partitionKey),
                ct);

            return course is null ? Results.NotFound() : Results.Ok(course);
        })
        .WithName("GetCourse")
        .WithTags("Courses")
        .HasApiVersion(1.0);

        group.MapPost("/", async (IDispatcher dispatcher, CreateCourseRequest request, CancellationToken ct) =>
        {
            var course = await dispatcher.SendAsync<CreateCourseCommand, Domain.Entities.Course>(
                new CreateCourseCommand(request.Title, request.Description, request.AuthorId),
                ct);

            return Results.Created($"/api/v1/courses/{course.Id}/{course.AuthorId}", course);
        })
        .WithName("CreateCourse")
        .WithTags("Courses")
        .HasApiVersion(1.0);

        group.MapPut("/{courseId}/{partitionKey}", async (IDispatcher dispatcher, string courseId, string partitionKey, UpdateCourseRequest request, CancellationToken ct) =>
        {
            var course = await dispatcher.SendAsync<UpdateCourseCommand, Domain.Entities.Course>(
                new UpdateCourseCommand(courseId, request.Title, request.Description, request.AuthorId),
                ct);

            return Results.Ok(course);
        })
        .WithName("UpdateCourse")
        .WithTags("Courses")
        .HasApiVersion(1.0);

        group.MapDelete("/{courseId}/{partitionKey}", async (IDispatcher dispatcher, string courseId, string partitionKey, CancellationToken ct) =>
        {
            var deleted = await dispatcher.SendAsync<DeleteCourseCommand, bool>(
                new DeleteCourseCommand(courseId, partitionKey),
                ct);

            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteCourse")
        .WithTags("Courses")
        .HasApiVersion(1.0);
    }

    public sealed record CreateCourseRequest(string Title, string Description, string AuthorId);
    public sealed record UpdateCourseRequest(string Title, string Description, string AuthorId);
}
