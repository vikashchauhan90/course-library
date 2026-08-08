using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace CourseLibrary.Gateway.Configuration.RateLimiting;

internal static class GatewayRateLimitingConstants
{
    public const string IpPolicyName = "IpRateLimit";
    public const string UserPolicyName = "UserRateLimit";
    public const string ConcurrentPolicyName = "ConcurrentRequestLimit";
}

internal static class GatewayRateLimitingExtensions
{
    public static WebApplicationBuilder AddGatewayRateLimiting(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.AddPolicy(GatewayRateLimitingConstants.IpPolicyName, httpContext =>
            {
                var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
                return RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey,
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 200,
                        TokensPerPeriod = 200,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        AutoReplenishment = true,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            options.AddPolicy(GatewayRateLimitingConstants.UserPolicyName, httpContext =>
            {
                var userKey = httpContext.User?.FindFirst("sub")?.Value
                    ?? httpContext.User?.Identity?.Name
                    ?? httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "anonymous";

                return RateLimitPartition.GetTokenBucketLimiter(
                    userKey,
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 120,
                        TokensPerPeriod = 120,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        AutoReplenishment = true,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            options.AddPolicy(GatewayRateLimitingConstants.ConcurrentPolicyName, _ =>
                RateLimitPartition.GetConcurrencyLimiter(
                    "global",
                    _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = 50,
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return builder;
    }
}
