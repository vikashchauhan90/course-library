using System.Threading.RateLimiting;

namespace CourseLibrary.Gateway.Configuration.RateLimiting;

internal static class GatewayRateLimitingExtensions
{
    public static WebApplicationBuilder AddGatewayRateLimiting(
        this WebApplicationBuilder builder)
    {
        var section = builder.Configuration
            .GetSection("RateLimiting");

        builder.Services.Configure<GatewayRateLimitingOptions>(section);

        var settings = section.Get<GatewayRateLimitingOptions>()
            ?? new GatewayRateLimitingOptions();

        builder.Services.AddRateLimiter(options =>
        {
            /*
             * IP-based Sliding Window
             */
            if (settings.Ip.Enabled)
            {
                options.AddPolicy(
                    settings.Ip.Name,
                    httpContext =>
                    {
                        var partitionKey =
                            httpContext.Connection.RemoteIpAddress?
                                .ToString()
                            ?? "unknown-ip";

                        return RateLimitPartition.GetSlidingWindowLimiter(
                            partitionKey,
                            _ => new SlidingWindowRateLimiterOptions
                            {
                                PermitLimit = settings.Ip.PermitLimit,

                                Window = TimeSpan.FromSeconds(
                                    settings.Ip.WindowSeconds),

                                SegmentsPerWindow =
                                    settings.Ip.SegmentsPerWindow,

                                QueueProcessingOrder =
                                    QueueProcessingOrder.OldestFirst,

                                QueueLimit =
                                    settings.Ip.QueueLimit
                            });
                    });
            }

            /*
             * User-based Sliding Window
             */
            if (settings.User.Enabled)
            {
                options.AddPolicy(
                    settings.User.Name,
                    httpContext =>
                    {
                        var userKey =
                            httpContext.User?
                                .FindFirst("sub")?.Value
                            ?? httpContext.User?
                                .Identity?.Name
                            ?? httpContext.Connection.RemoteIpAddress?
                                .ToString()
                            ?? "anonymous";

                        return RateLimitPartition.GetSlidingWindowLimiter(
                            userKey,
                            _ => new SlidingWindowRateLimiterOptions
                            {
                                PermitLimit =
                                    settings.User.PermitLimit,

                                Window = TimeSpan.FromSeconds(
                                    settings.User.WindowSeconds),

                                SegmentsPerWindow =
                                    settings.User.SegmentsPerWindow,

                                QueueProcessingOrder =
                                    QueueProcessingOrder.OldestFirst,

                                QueueLimit =
                                    settings.User.QueueLimit
                            });
                    });
            }

            /*
             * Global Concurrency Limit
             */
            if (settings.Concurrency.Enabled)
            {
                options.AddPolicy(
                    settings.Concurrency.Name,
                    _ =>
                        RateLimitPartition.GetConcurrencyLimiter(
                            "global",
                            _ => new ConcurrencyLimiterOptions
                            {
                                PermitLimit =
                                    settings.Concurrency.PermitLimit,

                                QueueLimit =
                                    settings.Concurrency.QueueLimit,

                                QueueProcessingOrder =
                                    QueueProcessingOrder.OldestFirst
                            }));
            }

            /*
             * Response when rate limit is exceeded.
             */
            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;
        });

        return builder;
    }
}
