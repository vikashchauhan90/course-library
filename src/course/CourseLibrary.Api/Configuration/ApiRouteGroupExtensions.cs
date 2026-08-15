namespace CourseLibrary.Api.Configuration;

public static class ApiRouteGroupExtensions
{
    public static RouteGroupBuilder MapApiVersionedGroup(this IEndpointRouteBuilder app, string resource)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(resource);

        return app.MapGroup($"/api/v{{version:apiVersion}}{resource}");
    }
}
