using CourseLibrary.Client.Courses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System.Net;

namespace CourseLibrary.Client.Configuration;

public static class CourseLibraryClientExtensions
{
    public static IServiceCollection AddCourseLibraryClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var gatewayBaseUrl = configuration["Gateway:BaseUrl"];
        if (!Uri.TryCreate(gatewayBaseUrl, UriKind.Absolute, out var baseUri) || baseUri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("Gateway:BaseUrl must be an absolute HTTPS URI.");

        services.ConfigureHttpClientDefaults(
             builder =>
             {
                 builder.AddStandardResilienceHandler(options =>
                 {
                     options.Retry.MaxRetryAttempts = 3;
                     options.Retry.ShouldRetryAfterHeader = true;
                     options.Retry.Delay = TimeSpan.FromSeconds(1);
                     options.Retry.BackoffType = DelayBackoffType.Exponential;
                     options.Retry.UseJitter = true;

                     options.Retry.ShouldHandle = static args =>
                     {
                         if (args.Outcome.Exception is HttpRequestException)
                         {
                             return new ValueTask<bool>(true);
                         }

                         if (args.Outcome.Result is HttpResponseMessage response)
                         {
                             return new ValueTask<bool>(
                                  response.StatusCode == HttpStatusCode.RequestTimeout ||
                                  response.StatusCode == HttpStatusCode.TooManyRequests ||
                                  (int)response.StatusCode >= (int)HttpStatusCode.InternalServerError);
                         }
                         return new ValueTask<bool>(false);
                     };

                     options.CircuitBreaker.FailureRatio = 0.20;
                     options.CircuitBreaker.MinimumThroughput = 20;
                     options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                     options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(10);

                     options.CircuitBreaker.ShouldHandle = static args =>
                     {
                         if (args.Outcome.Exception is HttpRequestException)
                         {
                             return new ValueTask<bool>(true);
                         }

                         if (args.Outcome.Result is HttpResponseMessage response)
                         {
                             return new ValueTask<bool>(
                                  response.StatusCode == HttpStatusCode.RequestTimeout ||
                                  (int)response.StatusCode >= (int)HttpStatusCode.InternalServerError);
                         }
                         return new ValueTask<bool>(false);
                     };
                 });
             });

        services.AddHttpClient<ICourseApiClient, CourseApiClient>(client =>
        {
            client.BaseAddress = baseUri;
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
