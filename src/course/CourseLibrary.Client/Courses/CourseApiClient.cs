using CourseLibrary.Client.Observability;
using CourseLibrary.Models;
using CourseLibrary.Models.Course;
using Hal.Core;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace CourseLibrary.Client.Courses;

internal sealed class CourseApiClient(
    HttpClient httpClient,
    ILogger<CourseApiClient> logger) : ICourseApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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
        (await SendAsync<IResource<CourseResponse>>(method, path, operation, cancellationToken));

    private async Task<IResource<CourseResponse>> SendResourceAsync(
        HttpMethod method,
        string path,
        object content,
        string operation,
        CancellationToken cancellationToken) =>
        (await SendAsync<IResource<CourseResponse>>(method, path, content, operation, cancellationToken));

    private async Task<IResource<PageResult<IResource<CourseResponse>>>> SendPageAsync(
        HttpMethod method,
        string path,
        string operation,
        CancellationToken cancellationToken)
    {
      return await SendAsync<IResource<PageResult<IResource<CourseResponse>>>>(method, path, operation, cancellationToken);
        
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

        if (!string.IsNullOrWhiteSpace(continuationToken))
            query.Add("pageToken", continuationToken.Trim());

        if (!string.IsNullOrWhiteSpace(criteria?.SearchTerm))
            query.Add("searchTerm", criteria.SearchTerm.Trim());

        if (!string.IsNullOrWhiteSpace(criteria?.AuthorId))
            query.Add("authorId", criteria.AuthorId);

        if (criteria?.IncludeDeleted == true)
            query.Add("includeDeleted", "true");

        if (criteria?.IncludeRetired == true)
            query.Add("includeRetired", "true");

        return $"api/v1/courses/{query}";
    }
}
