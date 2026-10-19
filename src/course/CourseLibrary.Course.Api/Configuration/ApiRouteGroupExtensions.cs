using Asp.Versioning;

namespace CourseLibrary.Course.Api.Configuration;

internal static class ApiRouteGroupExtensions
{
    public static RouteGroupBuilder MapApiV1Group(
        this IEndpointRouteBuilder endpoints,
        string resource,
        string tag)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        return endpoints
            .MapGroup($"/api/v{{version:apiVersion}}/{resource.Trim('/')}")
            .WithApiVersionSet(versionSet)
            .WithTags(tag);
    }
}
