using CourseLibrary.Infrastructure.Resilience;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;


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
                    UseJitter = true
                });

                builder.AddCircuitBreaker(
                    new CircuitBreakerStrategyOptions
                    {
                        FailureRatio = 0.10,
                        MinimumThroughput = 20,
                        SamplingDuration = TimeSpan.FromSeconds(30),
                        BreakDuration = TimeSpan.FromSeconds(10)
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
                    UseJitter = true
                });

                builder.AddCircuitBreaker(
                    new CircuitBreakerStrategyOptions
                    {
                        FailureRatio = 0.10,
                        MinimumThroughput = 20,
                        SamplingDuration = TimeSpan.FromSeconds(30),
                        BreakDuration = TimeSpan.FromSeconds(10)
                    });

                builder.AddTimeout(TimeSpan.FromSeconds(5));
            });

        return services;
    }
}
