namespace CourseLibrary.Platform.Resilience.Distributed;

public sealed class DistributedResilienceOptions
{
    public string KeyPrefix { get; set; } = "course-library:distributed-resilience";

    public TimeSpan StateRetention { get; set; } = TimeSpan.FromHours(1);

    public int MaxRetryAttempts { get; set; } = 3;

    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(30);

    public int MinimumThroughput { get; set; } = 20;

    public double FailureRatio { get; set; } = 0.20;

    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(10);

    public TimeSpan ProbeLeaseDuration { get; set; } = TimeSpan.FromSeconds(30);

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    internal void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(KeyPrefix);
        if (KeyPrefix.Contains('{') || KeyPrefix.Contains('}'))
        {
            throw new InvalidOperationException(
                $"{nameof(KeyPrefix)} cannot contain Redis hash-tag braces.");
        }

        if (StateRetention <= TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                $"{nameof(StateRetention)} must be greater than zero.");
        }

        if (MaxRetryAttempts < 0)
        {
            throw new InvalidOperationException(
                $"{nameof(MaxRetryAttempts)} cannot be negative.");
        }

        ValidatePositive(RetryDelay, nameof(RetryDelay));
        ValidatePositive(MaxRetryDelay, nameof(MaxRetryDelay));
        ValidatePositive(SamplingDuration, nameof(SamplingDuration));
        ValidatePositive(BreakDuration, nameof(BreakDuration));
        ValidatePositive(ProbeLeaseDuration, nameof(ProbeLeaseDuration));
        ValidatePositive(Timeout, nameof(Timeout));

        if (MinimumThroughput < 2)
        {
            throw new InvalidOperationException(
                $"{nameof(MinimumThroughput)} must be at least 2.");
        }

        if (!double.IsFinite(FailureRatio) || FailureRatio is <= 0 or > 1)
        {
            throw new InvalidOperationException(
                $"{nameof(FailureRatio)} must be greater than zero and no greater than one.");
        }

        if (MaxRetryDelay < RetryDelay)
        {
            throw new InvalidOperationException(
                $"{nameof(MaxRetryDelay)} must be greater than or equal to {nameof(RetryDelay)}.");
        }

        if (ProbeLeaseDuration < Timeout)
        {
            throw new InvalidOperationException(
                $"{nameof(ProbeLeaseDuration)} must be greater than or equal to {nameof(Timeout)}.");
        }
    }

    private static void ValidatePositive(TimeSpan value, string name)
    {
        if (value <= TimeSpan.Zero)
        {
            throw new InvalidOperationException($"{name} must be greater than zero.");
        }
    }
}
