using System.Diagnostics.Metrics;
using CourseLibrary.Platform.Observability;

namespace CourseLibrary.Course.EventConsumer.Observability;

internal sealed class CourseEventConsumerMeter : MeterBase
{
    private static readonly Lazy<CourseEventConsumerMeter> SharedInstance = new(
        static () => new CourseEventConsumerMeter(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static CourseEventConsumerMeter Shared => SharedInstance.Value;

    private readonly Counter<long> _messages;
    private readonly Counter<long> _failures;
    private readonly Counter<long> _duplicates;
    private readonly Counter<long> _orchestrations;
    private readonly Counter<long> _activities;
    private readonly Counter<long> _functionExecutions;
    private readonly UpDownCounter<long> _activeFunctions;
    private readonly Histogram<double> _messageDuration;
    private readonly Histogram<double> _functionDuration;
    private readonly Histogram<double> _activityDuration;

    private CourseEventConsumerMeter()
        : base(CourseEventConsumerTelemetry.MeterName)
    {
        _messages = CreateCounter<long>(
            "course.event_consumer.messages",
            "{message}",
            "Number of CourseEvent messages by outcome.");
        _failures = CreateCounter<long>(
            "course.event_consumer.failures",
            "{failure}",
            "Number of failed CourseEvent consumer operations.");
        _duplicates = CreateCounter<long>(
            "course.event_consumer.duplicates",
            "{message}",
            "Number of duplicate CourseEvent messages.");
        _orchestrations = CreateCounter<long>(
            "course.event_consumer.orchestrations",
            "{orchestration}",
            "Number of CourseEvent orchestrations started.");
        _activities = CreateCounter<long>(
            "course.event_consumer.activities",
            "{activity}",
            "Number of CourseEvent activities completed or failed.");
        _functionExecutions = CreateCounter<long>(
            "course.event_consumer.function.executions",
            "{execution}",
            "Number of Azure Functions executions.");
        _activeFunctions = CreateUpDownCounter<long>(
            "course.event_consumer.function.active",
            "{function}",
            "Number of currently executing Azure Functions.");
        _messageDuration = CreateHistogram<double>(
            "course.event_consumer.message.duration",
            "ms",
            "CourseEvent message processing duration.");
        _functionDuration = CreateHistogram<double>(
            "course.event_consumer.function.duration",
            "ms",
            "Azure Functions execution duration.");
        _activityDuration = CreateHistogram<double>(
            "course.event_consumer.activity.duration",
            "ms",
            "CourseEvent activity execution duration.");
    }

    public void RecordMessage(string outcome, double durationMilliseconds)
    {
        var tags = new TagList { { "outcome", outcome } };
        _messages.Add(1, tags);
        _messageDuration.Record(durationMilliseconds, tags);
    }

    public void RecordFailure(string operation, string failureType)
        => _failures.Add(1, new TagList
        {
            { "operation", operation },
            { "error.type", failureType }
        });

    public void RecordDuplicate(string status)
        => _duplicates.Add(1, new TagList { { "status", status } });

    public void RecordOrchestrationStarted(string name)
        => _orchestrations.Add(1, new TagList { { "orchestration.name", name } });

    public void RecordActivity(string name, string outcome, double durationMilliseconds)
    {
        var tags = new TagList
        {
            { "activity.name", name },
            { "outcome", outcome }
        };
        _activities.Add(1, tags);
        _activityDuration.Record(durationMilliseconds, tags);
    }

    public void RecordFunctionStarted(string name)
    {
        var tags = new TagList { { "function.name", name } };
        _activeFunctions.Add(1, tags);
    }

    public void RecordFunctionCompleted(string name, double durationMilliseconds, string outcome)
    {
        var tags = new TagList
        {
            { "function.name", name },
            { "outcome", outcome }
        };
        _activeFunctions.Add(-1, new TagList { { "function.name", name } });
        _functionExecutions.Add(1, tags);
        _functionDuration.Record(durationMilliseconds, tags);
    }
}
