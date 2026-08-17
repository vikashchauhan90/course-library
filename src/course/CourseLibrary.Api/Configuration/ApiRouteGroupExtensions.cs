using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;

namespace CourseLibrary.Api.Configuration;

public static class ApiRouteGroupExtensions
{
    public static RouteGroupBuilder MapApiVersionedGroup(
        this IEndpointRouteBuilder app,
        string resource)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(resource);

        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(1.0)
            .ReportApiVersions()
            .Build();

        return app
            .MapGroup($"/api/v{{version:apiVersion}}{resource}")
            .WithApiVersionSet(versionSet);
    }

    public static RouteGroupBuilder MapApiVersionedGroup(
        this IEndpointRouteBuilder app,
        string resource,
        string groupName)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(groupName);
        return app.MapGroup($"/api/v{{version:apiVersion}}{resource}")
            .WithGroupName(groupName);
    }

    public static RouteGroupBuilder MapApiVersionedGroup(
        this IEndpointRouteBuilder app,
        string resource,
        string groupName,
        string[] tags)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(groupName);
        ArgumentNullException.ThrowIfNull(tags);
        return app.MapGroup($"/api/v{{version:apiVersion}}{resource}")
            .WithGroupName(groupName)
            .WithTags(tags);
    }

    public static RouteGroupBuilder MapApiVersionedGroup(
        this IEndpointRouteBuilder app,
        string resource,
        string groupName,
        string[] tags,
        ApiVersionSet versionSet)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(groupName);
        ArgumentNullException.ThrowIfNull(tags);
        ArgumentNullException.ThrowIfNull(versionSet);
        return app.MapGroup($"/api/v{{version:apiVersion}}{resource}")
            .WithGroupName(groupName)
            .WithTags(tags)
            .WithApiVersionSet(versionSet);
    }
}
