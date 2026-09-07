using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CourseLibrary.Client.Observability;
using CourseLibrary.Client.Security;
using Microsoft.Extensions.Logging;
using CourseLibrary.Models;
using CourseLibrary.Models.Course;

namespace CourseLibrary.Client.Courses;

internal sealed class CourseApiClient(
    HttpClient httpClient,
    IAccessTokenProvider accessTokenProvider,
    ILogger<CourseApiClient> logger) : ICourseApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<CourseDetails?> GetCourseAsync(string courseId, string partitionKey, CancellationToken cancellationToken = default) =>
        SendAsync<CourseDetails?>(
            HttpMethod.Get,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}/{Uri.EscapeDataString(partitionKey)}",
            operation: "get-course",
            cancellationToken);

    public Task<PageResult<CourseDetails>> SearchAsync(
        string? query,
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default) =>
        SendAsync<PageResult<CourseDetails>>(
            HttpMethod.Get,
            $"api/v1/courses/?q={Uri.EscapeDataString(query?.Trim() ?? string.Empty)}&pageSize={pageSize}&pageToken={Uri.EscapeDataString(continuationToken ?? string.Empty)}",
            operation: "search-courses",
            cancellationToken);

    public Task<PageResult<CourseDetails>> GetMineAsync(
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default) =>
        SendAsync<PageResult<CourseDetails>>(
            HttpMethod.Get,
            $"api/v1/courses/?mine=true&pageSize={pageSize}&pageToken={Uri.EscapeDataString(continuationToken ?? string.Empty)}",
            operation: "get-my-courses",
            cancellationToken);

    public Task<CourseDetails> CreateAsync(CourseWriteRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<CourseDetails>(
            HttpMethod.Post,
            "api/v1/courses/",
            request,
            operation: "create-course",
            cancellationToken);

    public Task<CourseDetails> UpdateAsync(string courseId, string partitionKey, CourseWriteRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<CourseDetails>(
            HttpMethod.Put,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}/{Uri.EscapeDataString(partitionKey)}",
            request,
            operation: "update-course",
            cancellationToken);

    public async Task DeleteAsync(string courseId, string partitionKey, CancellationToken cancellationToken = default)
    {
        await SendAsync<object>(
            HttpMethod.Delete,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}/{Uri.EscapeDataString(partitionKey)}",
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
