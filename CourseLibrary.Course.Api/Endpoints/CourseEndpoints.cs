using Asp.Versioning.Builder;
using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.Operations.Courses.Create;
using CourseLibrary.Course.Application.Operations.Courses.Delete;
using CourseLibrary.Course.Application.Operations.Courses.Get;
using CourseLibrary.Course.Application.Operations.Courses.Retire;
using CourseLibrary.Course.Application.Operations.Courses.Search;
using CourseLibrary.Course.Application.Operations.Courses.Update;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Contracts.Pagination;
using CourseLibrary.Platform.AspNetCore.RequestContext;

namespace CourseLibrary.Course.Api.Endpoints;

public sealed class CourseEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("courses", "Courses");

        group.MapGet("/", async (
                HttpRequest request,
                IRequestContext requestContext,
                ISearchCoursesOperation operation,
                CancellationToken cancellationToken) =>
            {
                var pageSizeText = request.Query["pageSize"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(pageSizeText) &&
                    !int.TryParse(pageSizeText, out _))
                {
                    return Results.BadRequest(new
                    {
                        title = "Invalid page size",
                        detail = "The pageSize query parameter must be an integer."
                    });
                }

                var pageSize = ParseInt(pageSizeText, 20);
                var searchTerm = request.Query["name"].FirstOrDefault();
                var mine = ParseBool(request.Query["mine"]);
                if (mine && string.IsNullOrWhiteSpace(requestContext.UserId))
                {
                    return Results.Unauthorized();
                }

                var criteria = new CourseSearchCriteria(
                    searchTerm,
                    mine ? requestContext.UserId : null,
                    ParseBool(request.Query["includeDeleted"]),
                    ParseBool(request.Query["includeRetired"]));

                var result = await operation.ExecuteAsync(
                    criteria,
                    pageSize,
                    request.Query["pageToken"].FirstOrDefault(),
                    cancellationToken);
                return result.ToHttpResult();
            })
            .WithName("GetCourses")
            .HasApiVersion(1.0);

        group.MapGet("/{courseId}", async (
                string courseId,
                IGetCourseOperation operation,
                CancellationToken cancellationToken) =>
                (await operation.ExecuteAsync(courseId, cancellationToken)).ToHttpResult())
            .WithName("GetCourse")
            .HasApiVersion(1.0);

        group.MapPost("/", async (
                CreateCourseRequest request,
                IRequestContext requestContext,
                ICreateCourseOperation operation,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(requestContext.UserId))
                {
                    return Results.Unauthorized();
                }

                var result = await operation.ExecuteAsync(request, requestContext.UserId, cancellationToken);
                return result.ToHttpResult(course =>
                    Results.CreatedAtRoute("GetCourse", new { version = "1", courseId = course?.Id }, course));
            })
            .WithName("CreateCourse")
            .HasApiVersion(1.0);

        group.MapPut("/{courseId}", async (
                string courseId,
                UpdateCourseRequest request,
                IRequestContext requestContext,
                IUpdateCourseOperation operation,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(requestContext.UserId))
                {
                    return Results.Unauthorized();
                }

                return (await operation.ExecuteAsync(
                    courseId,
                    request,
                    requestContext.UserId,
                    cancellationToken)).ToHttpResult();
            })
            .WithName("UpdateCourse")
            .HasApiVersion(1.0);

        group.MapPost("/{courseId}/retire", async (
                string courseId,
                IRequestContext requestContext,
                IRetireCourseOperation operation,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(requestContext.UserId))
                {
                    return Results.Unauthorized();
                }

                return (await operation.ExecuteAsync(
                    courseId,
                    requestContext.UserId,
                    cancellationToken)).ToHttpResult();
            })
            .WithName("RetireCourse")
            .HasApiVersion(1.0);

        group.MapDelete("/{courseId}", async (
                string courseId,
                IRequestContext requestContext,
                IDeleteCourseOperation operation,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(requestContext.UserId))
                {
                    return Results.Unauthorized();
                }

                var result = await operation.ExecuteAsync(courseId, requestContext.UserId, cancellationToken);
                return result.ToHttpResult(deleted => deleted
                    ? Results.NoContent()
                    : Results.NotFound());
            })
            .WithName("DeleteCourse")
            .HasApiVersion(1.0);
    }

    private static int ParseInt(string? value, int defaultValue)
        => int.TryParse(value, out var parsed) ? parsed : defaultValue;

    private static bool ParseBool(string? value)
        => bool.TryParse(value, out var parsed) && parsed;
}
