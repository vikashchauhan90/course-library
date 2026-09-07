using CourseLibrary.Client.Observability;
using CourseLibrary.Client.Security;
using CourseLibrary.Models;
using CourseLibrary.Models.Course;
using Hal.Core;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CourseLibrary.Client.Courses;

internal sealed class CourseApiClient(
    HttpClient httpClient,
    IAccessTokenProvider accessTokenProvider,
    ILogger<CourseApiClient> logger) : ICourseApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IResource<CourseDetails>> GetCourseAsync(string courseId, CancellationToken cancellationToken = default) =>
        await SendResourceAsync(
            HttpMethod.Get,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}",
            operation: "get-course",
            cancellationToken);

    public Task<IResource<PageResult<IResource<CourseDetails>>>> SearchAsync(
        CourseSearchCriteria? criteria,
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default) =>
        SendPageAsync(
            HttpMethod.Get,
            BuildSearchPath(criteria, pageSize, continuationToken),
            operation: "search-courses",
            cancellationToken);

    public Task<IResource<PageResult<IResource<CourseDetails>>>> GetMineAsync(
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default) =>
        SendPageAsync(
            HttpMethod.Get,
            $"api/v1/courses/?mine=true&pageSize={pageSize}&pageToken={Uri.EscapeDataString(continuationToken ?? string.Empty)}",
            operation: "get-my-courses",
            cancellationToken);


private static string BuildSearchPath(
    CourseSearchCriteria? criteria,
    int pageSize,
    string? continuationToken)
{
    var query = new QueryBuilder
    {
        { "pageSize", pageSize.ToString() },
        { "pageToken", continuationToken ?? string.Empty }
    };

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


public async Task<IResource<CourseDetails>> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken = default) =>
        await SendResourceAsync(
            HttpMethod.Post,
            "api/v1/courses/",
            request,
            operation: "create-course",
            cancellationToken);

    public async Task<IResource<CourseDetails>> UpdateAsync(string courseId, UpdateCourseRequest request, CancellationToken cancellationToken = default) =>
        await SendResourceAsync(
            HttpMethod.Put,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}",
            request,
            operation: "update-course",
            cancellationToken);

    public async Task<IResource<CourseDetails>> RetireAsync(string courseId, CancellationToken cancellationToken = default) =>
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

    private async Task<IResource<CourseDetails>> SendResourceAsync(
        HttpMethod method,
        string path,
        string operation,
        CancellationToken cancellationToken) =>
        (await SendAsync<HalDocument<CourseDetails>>(method, path, operation, cancellationToken)).ToResource();

    private async Task<IResource<CourseDetails>> SendResourceAsync(
        HttpMethod method,
        string path,
        object content,
        string operation,
        CancellationToken cancellationToken) =>
        (await SendAsync<HalDocument<CourseDetails>>(method, path, content, operation, cancellationToken)).ToResource();

    private async Task<IResource<PageResult<IResource<CourseDetails>>>> SendPageAsync(
        HttpMethod method,
        string path,
        string operation,
        CancellationToken cancellationToken)
    {
        var document = await SendAsync<HalDocument<PageResult<HalDocument<CourseDetails>>>>(method, path, operation, cancellationToken);
        if (document.Data is null)
            throw new JsonException("The HAL response did not contain a page payload.");

        var items = document.Data.Items
            .Select(item => item.ToResource())
            .ToList();
        var page = new PageResult<IResource<CourseDetails>>(
            items,
            document.Data.ContinuationToken,
            document.Data.HasMore);
        return new HalDocument<PageResult<IResource<CourseDetails>>>
        {
            Data = page,
            Links = document.Links
        }.ToResource();
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
            var accessToken = await accessTokenProvider.GetAccessTokenAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new InvalidOperationException("The current session has no access token.");

            using var request = new HttpRequestMessage(method, path)
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

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
}
