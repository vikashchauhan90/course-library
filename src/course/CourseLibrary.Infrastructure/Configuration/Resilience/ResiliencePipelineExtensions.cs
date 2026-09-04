using System.Net;
using CourseLibrary.Infrastructure.Resilience;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace CourseLibrary.Infrastructure.Configuration.Resilience;

public static class ResiliencePipelineExtensions
{
    public static IServiceCollection AddCourseLibraryResilience(
        this IServiceCollection services)
    {
        services.AddResiliencePipeline(
            ResiliencePolicies.ExternalDependency,
            static builder =>
            {
                builder.AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,

                    ShouldHandle = static args =>
                    {
                        if (args.Outcome.Exception is HttpRequestException)
                        {
                            return new ValueTask<bool>(true);
                        }

                        if (args.Outcome.Exception is TimeoutRejectedException)
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
                    },

                    DelayGenerator = static args =>
                    {
                        if (args.Outcome.Result is not HttpResponseMessage response ||
                            response.StatusCode != HttpStatusCode.TooManyRequests)
                        {
                            return new ValueTask<TimeSpan?>((TimeSpan?)null);
                        }

                        var retryAfter = response.Headers.RetryAfter;

                        // Retry-After: <delta-seconds>
                        if (retryAfter?.Delta is TimeSpan delta &&
                            delta >= TimeSpan.Zero)
                        {
                            return new ValueTask<TimeSpan?>(delta);
                        }

                        // Retry-After: <HTTP-date>
                        if (retryAfter?.Date is DateTimeOffset date)
                        {
                            var delay = date - DateTimeOffset.UtcNow;

                            if (delay > TimeSpan.Zero)
                            {
                                return new ValueTask<TimeSpan?>(delay);
                            }

                            return new ValueTask<TimeSpan?>(TimeSpan.Zero);
                        }

                        // No Retry-After header.
                        // Polly falls back to the configured
                        // exponential backoff + jitter.
                        return new ValueTask<TimeSpan?>((TimeSpan?)null);
                    }
                });

                builder.AddCircuitBreaker(
                    new CircuitBreakerStrategyOptions
                    {
                        FailureRatio = 0.20,
                        MinimumThroughput = 20,
                        SamplingDuration = TimeSpan.FromSeconds(30),
                        BreakDuration = TimeSpan.FromSeconds(10),

                        ShouldHandle = static args =>
                        {
                            if (args.Outcome.Exception is HttpRequestException)
                            {
                                return new ValueTask<bool>(true);
                            }

                            if (args.Outcome.Exception is TimeoutRejectedException)
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
                        }
                    });

                builder.AddTimeout(TimeSpan.FromSeconds(10));
            });

        services.AddResiliencePipeline(
            ResiliencePolicies.CriticalDependency,
            static builder =>
            {
                builder.AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 2,
                    Delay = TimeSpan.FromMilliseconds(500),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,

                    ShouldHandle = static args =>
                    {
                        if (args.Outcome.Exception is HttpRequestException)
                        {
                            return new ValueTask<bool>(true);
                        }

                        if (args.Outcome.Exception is TimeoutRejectedException)
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
                    },

                    DelayGenerator = static args =>
                    {
                        if (args.Outcome.Result is not HttpResponseMessage response ||
                            response.StatusCode != HttpStatusCode.TooManyRequests)
                        {
                            return new ValueTask<TimeSpan?>((TimeSpan?)null);
                        }

                        var retryAfter = response.Headers.RetryAfter;

                        // Retry-After: <delta-seconds>
                        if (retryAfter?.Delta is TimeSpan delta &&
                            delta >= TimeSpan.Zero)
                        {
                            return new ValueTask<TimeSpan?>(delta);
                        }

                        // Retry-After: <HTTP-date>
                        if (retryAfter?.Date is DateTimeOffset date)
                        {
                            var delay = date - DateTimeOffset.UtcNow;

                            if (delay > TimeSpan.Zero)
                            {
                                return new ValueTask<TimeSpan?>(delay);
                            }

                            return new ValueTask<TimeSpan?>(TimeSpan.Zero);
                        }

                        // No Retry-After header.
                        return new ValueTask<TimeSpan?>((TimeSpan?)null);
                    }
                });

                builder.AddCircuitBreaker(
                    new CircuitBreakerStrategyOptions
                    {
                        FailureRatio = 0.10,
                        MinimumThroughput = 20,
                        SamplingDuration = TimeSpan.FromSeconds(30),
                        BreakDuration = TimeSpan.FromSeconds(10),

                        ShouldHandle = static args =>
                        {
                            if (args.Outcome.Exception is HttpRequestException)
                            {
                                return new ValueTask<bool>(true);
                            }

                            if (args.Outcome.Exception is TimeoutRejectedException)
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
                        }
                    });

                builder.AddTimeout(TimeSpan.FromSeconds(5));
            });

        services.AddSingleton<PolicyFactory>();

        return services;
    }
}