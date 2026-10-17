using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Course.EventConsumer.RateLimiting;

internal sealed class CourseEventRateLimitingOptions
{
    public const string SectionName = "CourseEventRateLimiting";

    [Range(1, 20)]
    public int MaxOperationsPerSecond { get; init; } = 20;
}

internal sealed class CourseEventRateLimiter : IDisposable
{
    private readonly TokenBucketRateLimiter _messageIntakeLimiter;
    private readonly TokenBucketRateLimiter _auditWriteLimiter;

    public CourseEventRateLimiter(IOptions<CourseEventRateLimitingOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var rate = options.Value.MaxOperationsPerSecond;
        _messageIntakeLimiter = CreateLimiter(rate, queueLimit: 20);
        _auditWriteLimiter = CreateLimiter(rate, queueLimit: 32);
    }

    public ValueTask WaitForMessagePermitAsync(CancellationToken cancellationToken)
        => WaitForPermitAsync(_messageIntakeLimiter, cancellationToken);

    public ValueTask WaitForAuditWritePermitAsync(CancellationToken cancellationToken)
        => WaitForPermitAsync(_auditWriteLimiter, cancellationToken);

    public void Dispose()
    {
        _messageIntakeLimiter.Dispose();
        _auditWriteLimiter.Dispose();
    }

    private static TokenBucketRateLimiter CreateLimiter(int operationsPerSecond, int queueLimit)
        => new(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 1,
            TokensPerPeriod = 1,
            ReplenishmentPeriod = TimeSpan.FromTicks(
                (TimeSpan.TicksPerSecond + operationsPerSecond - 1) / operationsPerSecond),
            AutoReplenishment = true,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = queueLimit
        });

    private static async ValueTask WaitForPermitAsync(
        TokenBucketRateLimiter limiter,
        CancellationToken cancellationToken)
    {
        using var lease = await limiter.AcquireAsync(1, cancellationToken);
        if (!lease.IsAcquired)
        {
            throw new InvalidOperationException(
                "The course event rate limiter queue is full; no operation permit was granted.");
        }
    }
}

internal static class CourseEventRateLimitingExtensions
{
    public static IServiceCollection AddCourseEventRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<CourseEventRateLimitingOptions>()
            .Bind(configuration.GetSection(CourseEventRateLimitingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<CourseEventRateLimiter>();
        return services;
    }
}
