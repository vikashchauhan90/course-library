using CourseLibrary.Client.Observability;
using CourseLibrary.Models;
using CourseLibrary.Models.Course;
using Hal.Core;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseLibrary.Client.Courses;

internal sealed class CourseApiClient(
    HttpClient httpClient,
    ILogger<CourseApiClient> logger) : ICourseApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public async Task<IResource<CourseResponse>> GetCourseAsync(string courseId, CancellationToken cancellationToken = default) =>
        await SendResourceAsync(
            HttpMethod.Get,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}",
            operation: "get-course",
            cancellationToken);

    public Task<IResource<PageResult<IResource<CourseResponse>>>> SearchAsync(
        CourseSearchCriteria? criteria,
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default) =>
        SendPageAsync(
            HttpMethod.Get,
            BuildSearchPath(criteria, pageSize, continuationToken),
            operation: "search-courses",
            cancellationToken);


    public async Task<IResource<CourseResponse>> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken = default) =>
            await SendResourceAsync(
                HttpMethod.Post,
                "api/v1/courses",
                request,
                operation: "create-course",
                cancellationToken);

    public async Task<IResource<CourseResponse>> UpdateAsync(string courseId, UpdateCourseRequest request, CancellationToken cancellationToken = default) =>
        await SendResourceAsync(
            HttpMethod.Put,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}",
            request,
            operation: "update-course",
            cancellationToken);

    public async Task<IResource<CourseResponse>> RetireAsync(string courseId, CancellationToken cancellationToken = default) =>
        await SendResourceAsync(
            HttpMethod.Post,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}/retire",
            operation: "retire-course",
            cancellationToken);

    public async Task DeleteAsync(string courseId, CancellationToken cancellationToken = default)
    {
        await SendAsync<object>(
            HttpMethod.Delete,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}",
            operation: "delete-course",
            cancellationToken);
    }

    private Task<T> SendAsync<T>(
        HttpMethod method,
        string path,
        string operation,
        CancellationToken cancellationToken) =>
        SendAsync<T>(method, path, content: null, operation, cancellationToken);

    private Task<T> SendAsync<T>(
        HttpMethod method,
        string path,
        object content,
        string operation,
        CancellationToken cancellationToken) =>
        SendAsync<T>(method, path, JsonContent.Create(content, options: JsonOptions), operation, cancellationToken);

    private async Task<IResource<CourseResponse>> SendResourceAsync(
        HttpMethod method,
        string path,
        string operation,
        CancellationToken cancellationToken) =>
        await SendAsync<HalDocument<CourseResponse>>(method, path, operation, cancellationToken);

    private async Task<IResource<CourseResponse>> SendResourceAsync(
        HttpMethod method,
        string path,
        object content,
        string operation,
        CancellationToken cancellationToken) =>
        await SendAsync<HalDocument<CourseResponse>>(method, path, content, operation, cancellationToken);

    private async Task<IResource<PageResult<IResource<CourseResponse>>>> SendPageAsync(
        HttpMethod method,
        string path,
        string operation,
        CancellationToken cancellationToken)
    {
        var document = await SendAsync<HalDocument<PageResult<HalDocument<CourseResponse>>>>(
            method,
            path,
            operation,
            cancellationToken);

        var result = new HalDocument<PageResult<IResource<CourseResponse>>>
        {
            Data = document.Data.Map(item => (IResource<CourseResponse>)item)
        };

        foreach (var link in document.Links)
            result.AddLink(link);

        foreach (var embeddedResource in document.EmbeddedResources)
            result.EmbeddedResources[embeddedResource.Key] = embeddedResource.Value;

        return result;

    }

    private async Task<T> SendAsync<T>(
        HttpMethod method,
        string path,
        HttpContent? content,
        string operation,
        CancellationToken cancellationToken)
    {
        using var activity = CourseApiDiagnostics.ActivitySource.StartActivity(
            $"CourseApi {operation}",
            ActivityKind.Client);
        var stopwatch = Stopwatch.StartNew();
        CourseApiDiagnostics.Requests.Add(1, new KeyValuePair<string, object?>("operation", operation));
        logger.Calling(operation);

        try
        {
            using var request = new HttpRequestMessage(method, path)
            {
                Content = content
            };

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                CourseApiDiagnostics.Failures.Add(1, new KeyValuePair<string, object?>("operation", operation));
                logger.Failed(operation, (int)response.StatusCode);
                var detail = await response.Content.ReadAsStringAsync(cancellationToken);
                CourseApiProblemDetails? problemDetails = null;
                try
                {
                    problemDetails = JsonSerializer.Deserialize<CourseApiProblemDetails>(detail, JsonOptions);
                }
                catch (JsonException)
                {
                    // Preserve the raw response when the server did not return RFC 7807 JSON.
                }

                throw new CourseApiException(response.StatusCode, operation, problemDetails, detail);
            }

            if (typeof(T) == typeof(object) || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return default!;

            return (await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken))!;
        }
        catch (CourseApiException)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            CourseApiDiagnostics.Failures.Add(1, new KeyValuePair<string, object?>("operation", operation));
            logger.FailedUnexpectedly(exception, operation);
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            throw;
        }
        finally
        {
            CourseApiDiagnostics.Duration.Record(
                stopwatch.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("operation", operation));
        }
    }


    private static string BuildSearchPath(
        CourseSearchCriteria? criteria,
        int pageSize,
        string? continuationToken)
    {
        var query = new QueryBuilder
    {
        { "pageSize", pageSize.ToString() }
    };

        AddIfNotNullOrWhiteSpace(query, "pageToken", continuationToken);
        AddIfNotNullOrWhiteSpace(query, "searchTerm", criteria?.SearchTerm);
        AddIfNotNullOrWhiteSpace(query, "authorId", criteria?.AuthorId);
        AddIfHasValue(query, "includeDeleted", criteria?.IncludeDeleted);
        AddIfHasValue(query, "includeRetired", criteria?.IncludeRetired);

        return $"api/v1/courses/{query}";
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new HalDocumentJsonConverterFactory());
        return options;
    }

    private static void AddIfNotNullOrWhiteSpace(QueryBuilder query, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            query.Add(key, value.Trim());
    }
    private static void AddIfHasValue(QueryBuilder query, string key, object? value)
    {
        if (value != null)
            query.Add(key, value.ToString());
    }
}
