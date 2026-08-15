namespace CourseLibrary.Gateway.Configuration.RateLimiting;

public sealed class GatewayRateLimitingOptions
{
    public SlidingWindowPolicyOptions Ip { get; set; } = new();
    public SlidingWindowPolicyOptions User { get; set; } = new();
    public ConcurrencyPolicyOptions Concurrency { get; set; } = new();
}

public sealed class SlidingWindowPolicyOptions
{
    public string Name { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public int PermitLimit { get; set; }

    public int WindowSeconds { get; set; }

    public int SegmentsPerWindow { get; set; } = 6;

    public int QueueLimit { get; set; }
}

public sealed class ConcurrencyPolicyOptions
{
    public string Name { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public int PermitLimit { get; set; }

    public int QueueLimit { get; set; }
}