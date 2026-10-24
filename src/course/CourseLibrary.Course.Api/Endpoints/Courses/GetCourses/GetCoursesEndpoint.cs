using Carter;
using CourseLibrary.Course.Api.Configuration;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Application.UseCases.Courses.Search;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Platform.AspNetCore.RequestContext;

namespace CourseLibrary.Course.Api.Endpoints.Courses.GetCourses;

public sealed class GetCoursesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiV1Group("courses", "Courses");

        group.MapGet("/", async (
                HttpRequest request,
                IRequestContext requestContext,
                ISearchCoursesUseCase useCase,
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

                var result = await useCase.ExecuteAsync(
                    criteria,
                    pageSize,
                    request.Query["pageToken"].FirstOrDefault(),
                    cancellationToken);
                return result.ToHttpResult();
            })
            .WithName("GetCourses")
            .HasApiVersion(1.0);
    }

    private static int ParseInt(string? value, int defaultValue)
        => int.TryParse(value, out var parsed) ? parsed : defaultValue;

    private static bool ParseBool(string? value)
        => bool.TryParse(value, out var parsed) && parsed;
}
