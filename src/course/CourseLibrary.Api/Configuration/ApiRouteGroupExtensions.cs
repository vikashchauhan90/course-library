using Asp.Versioning;
using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;

namespace CourseLibrary.Api.Configuration;

public static class ApiRouteGroupExtensions
{
    private const string ApiPrefix = "/api/v{version:apiVersion}";

    public static RouteGroupBuilder MapApiVersionedGroup(
        this IEndpointRouteBuilder app,
        string resource,
        string? groupName = null,
        string[]? tags = null,
        ApiVersionSet? versionSet = null)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);

        resource = NormalizeResource(resource);

        versionSet ??= app.NewApiVersionSet()
            .HasApiVersion(1.0)
            .ReportApiVersions()
            .Build();

        var group = app.MapGroup($"{ApiPrefix}{resource}")
            .WithApiVersionSet(versionSet);

        if (!string.IsNullOrWhiteSpace(groupName))
        {
            group.WithGroupName(groupName);
        }

        if (tags is { Length: > 0 })
        {
            group.WithTags(tags);
        }

        return group;
    }

    private static string NormalizeResource(string resource)
    {
        return resource.StartsWith('/')
            ? resource
            : $"/{resource}";
    }
}