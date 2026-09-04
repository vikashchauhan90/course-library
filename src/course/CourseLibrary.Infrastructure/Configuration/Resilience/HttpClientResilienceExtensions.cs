using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ServiceDiscovery;
using Polly;
using System.Net;

namespace CourseLibrary.Infrastructure.Configuration.Resilience;

public static class HttpClientResilienceExtensions
{
    public static IServiceCollection AddCourseLibraryHttpResilience(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ServiceDiscoveryOptions>(
           static options =>
           {
               options.AllowAllSchemes = false;
               options.AllowedSchemes = ["https"];
           });

        services.AddServiceDiscovery();
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
                builder.AddServiceDiscovery();
            });

        services.AddCourseLibraryResilience();
        return services;
    }
}