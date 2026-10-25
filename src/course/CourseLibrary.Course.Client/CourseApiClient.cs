using CourseLibrary.Core.Results;
using CourseLibrary.Course.Client.Observability;
using CourseLibrary.Course.Contracts.DTOs;
using CourseLibrary.Course.Contracts.DTOs.Courses;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseLibrary.Course.Client;

internal sealed class CourseApiClient(
    HttpClient httpClient,
    ILogger<CourseApiClient> logger) : ICourseApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private static readonly CourseClientActivitySource ActivitySources = CourseClientActivitySource.Shared;
    private static readonly CourseClientMeter CourseApiMeter = CourseClientMeter.Shared;

    public Task<CourseResponse> GetCourseAsync(
        string courseId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(courseId);

        return SendResourceAsync(
            HttpMethod.Get,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}",
            operation: "get-course",
            cancellationToken);
    }

    public Task<PageResult<CourseResponse>> SearchAsync(
        CourseSearchCriteria? criteria,
        int pageSize = 20,
        string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        return SendPageAsync(
            HttpMethod.Get,
            BuildSearchPath(criteria, pageSize, continuationToken),
            operation: "search-courses",
            cancellationToken);
    }

    public Task<CourseResponse> CreateAsync(
        CourseRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return SendResourceAsync(
            HttpMethod.Post,
            "api/v1/courses",
            request,
            operation: "create-course",
            cancellationToken);
    }

    public Task<CourseResponse> UpdateAsync(
        string courseId,
        UpdateCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(courseId);
        ArgumentNullException.ThrowIfNull(request);

        return SendResourceAsync(
            HttpMethod.Put,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}",
            request,
            operation: "update-course",
            cancellationToken);
    }

    public Task<CourseResponse> RetireAsync(
        string courseId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(courseId);

        return SendResourceAsync(
            HttpMethod.Post,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}/retire",
            operation: "retire-course",
            cancellationToken);
    }

    public async Task DeleteAsync(
        string courseId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(courseId);

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
        SendAsync<T>(
            method,
            path,
            JsonContent.Create(content, options: JsonOptions),
            operation,
            cancellationToken);

    private Task<CourseResponse> SendResourceAsync(
        HttpMethod method,
        string path,
        string operation,
        CancellationToken cancellationToken)
        => SendAsync<CourseResponse>(
            method,
            path,
            operation,
            cancellationToken);

    private Task<CourseResponse> SendResourceAsync(
        HttpMethod method,
        string path,
        object content,
        string operation,
        CancellationToken cancellationToken)
        => SendAsync<CourseResponse>(
            method,
            path,
            content,
            operation,
            cancellationToken);

    private Task<PageResult<CourseResponse>> SendPageAsync(
        HttpMethod method,
        string path,
        string operation,
        CancellationToken cancellationToken)
        => SendAsync<PageResult<CourseResponse>>(
            method,
            path,
            operation,
            cancellationToken);

    private async Task<T> SendAsync<T>(
        HttpMethod method,
        string path,
        HttpContent? content,
        string operation,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySources.StartActivity(
            $"CourseApi {operation}",
            ActivityKind.Client);
        activity?.SetTag(CourseClientActivityTagConstants.Operation, operation);
        activity?.SetTag(CourseClientActivityTagConstants.HttpRequestMethod, method.Method);

        var stopwatch = Stopwatch.StartNew();
        CourseApiMeter.RecordRequest(operation);
        logger.Calling(operation);

        try
        {
            using var request = new HttpRequestMessage(method, path)
            {
                Content = content
            };

            using var response = await httpClient.SendAsync(request, cancellationToken);
            activity?.SetTag(
                CourseClientActivityTagConstants.HttpResponseStatusCode,
                (int)response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync(cancellationToken);
                CourseApiProblemDetails? problemDetails = null;
                try
                {
                    problemDetails = JsonSerializer.Deserialize<CourseApiProblemDetails>(detail, JsonOptions);
                }
                catch (JsonException)
                {
                    // Keep the raw response detail when the server did not return problem-details JSON.
                }

                throw new CourseApiException(response.StatusCode, operation, problemDetails, detail);
            }

            T result;
            if (typeof(T) == typeof(object))
            {
                result = default!;
            }
            else if (response.StatusCode == HttpStatusCode.NoContent)
            {
                throw new JsonException($"The Course API {operation} response did not contain the expected response body.");
            }
            else
            {
                result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
                    ?? throw new JsonException($"The Course API {operation} response body was empty.");
            }

            activity?.SetTag(CourseClientActivityTagConstants.OperationSuccess, true);
            activity?.SetStatus(ActivityStatusCode.Ok);
            logger.Completed(operation, (int)response.StatusCode, stopwatch.Elapsed.TotalMilliseconds);

            return result;
        }
        catch (CourseApiException exception)
        {
            CourseApiMeter.RecordFailure(operation, CourseClientActivityTagConstants.HttpFailure);
            activity?.SetTag(CourseClientActivityTagConstants.FailureType, CourseClientActivityTagConstants.HttpFailure);
            activity?.SetTag(
                CourseClientActivityTagConstants.HttpResponseStatusCode,
                (int)exception.StatusCode);
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            logger.Failed(operation, (int)exception.StatusCode);
            throw;
        }
        catch (OperationCanceledException)
        {
            CourseApiMeter.RecordFailure(operation, CourseClientActivityTagConstants.CancelledFailure);
            activity?.SetTag(CourseClientActivityTagConstants.OperationCancelled, true);
            activity?.SetTag(CourseClientActivityTagConstants.FailureType, CourseClientActivityTagConstants.CancelledFailure);
            activity?.SetStatus(ActivityStatusCode.Error, "Operation was cancelled.");
            logger.Cancelled(operation);
            throw;
        }
        catch (Exception exception)
        {
            CourseApiMeter.RecordFailure(operation, CourseClientActivityTagConstants.ClientFailure);
            activity?.SetTag(CourseClientActivityTagConstants.FailureType, CourseClientActivityTagConstants.ClientFailure);
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            logger.FailedUnexpectedly(exception, operation);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            CourseApiMeter.RecordDuration(stopwatch.Elapsed.TotalMilliseconds, operation);
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
        return options;
    }

    private static void AddIfNotNullOrWhiteSpace(QueryBuilder query, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add(key, value.Trim());
        }
    }

    private static void AddIfHasValue(QueryBuilder query, string key, object? value)
    {
        if (value is not null)
        {
            query.Add(key, value.ToString());
        }
    }

}
