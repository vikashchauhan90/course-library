using CourseLibrary.Client.Courses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

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

        services.AddHttpClient<ICourseApiClient, CourseApiClient>(client =>
            {
                client.BaseAddress = baseUri;
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(1);
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.UseJitter = true;
                options.Retry.DisableForUnsafeHttpMethods();
                options.Retry.ShouldHandle = static args =>
                {
                    if (args.Outcome.Exception is HttpRequestException)
                        return new ValueTask<bool>(true);

                    if (args.Outcome.Result is HttpResponseMessage response)
                    {
                        return new ValueTask<bool>(
                            response.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
                            response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                            (int)response.StatusCode >= 500);
                    }

                    return new ValueTask<bool>(false);
                };
                options.CircuitBreaker.ShouldHandle = static args =>
                {
                    if (args.Outcome.Exception is HttpRequestException)
                        return new ValueTask<bool>(true);

                    return new ValueTask<bool>(
                        args.Outcome.Result is HttpResponseMessage response && (int)response.StatusCode >= 500);
                };
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            });

        return services;
    }
}
