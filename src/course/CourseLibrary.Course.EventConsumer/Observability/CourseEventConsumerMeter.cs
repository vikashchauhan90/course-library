using CourseLibrary.Platform.Observability;
using System.Diagnostics;
using System.Diagnostics.Metrics;

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
            CourseEventConsumerMeterConstants.Messages,
            "{message}",
            "Number of CourseEvent messages by outcome.");
        _failures = CreateCounter<long>(
            CourseEventConsumerMeterConstants.Failures,
            "{failure}",
            "Number of failed CourseEvent consumer operations.");
        _duplicates = CreateCounter<long>(
            CourseEventConsumerMeterConstants.Duplicates,
            "{message}",
            "Number of duplicate CourseEvent messages.");
        _orchestrations = CreateCounter<long>(
            CourseEventConsumerMeterConstants.Orchestrations,
            "{orchestration}",
            "Number of CourseEvent orchestrations started.");
        _activities = CreateCounter<long>(
            CourseEventConsumerMeterConstants.Activities,
            "{activity}",
            "Number of CourseEvent activities completed or failed.");
        _functionExecutions = CreateCounter<long>(
            CourseEventConsumerMeterConstants.FunctionExecutions,
            "{execution}",
            "Number of Azure Functions executions.");
        _activeFunctions = CreateUpDownCounter<long>(
            CourseEventConsumerMeterConstants.ActiveFunctions,
            "{function}",
            "Number of currently executing Azure Functions.");
        _messageDuration = CreateHistogram<double>(
            CourseEventConsumerMeterConstants.MessageDuration,
            "ms",
            "CourseEvent message processing duration.");
        _functionDuration = CreateHistogram<double>(
            CourseEventConsumerMeterConstants.FunctionDuration,
            "ms",
            "Azure Functions execution duration.");
        _activityDuration = CreateHistogram<double>(
            CourseEventConsumerMeterConstants.ActivityDuration,
            "ms",
            "CourseEvent activity execution duration.");
    }

    public void RecordMessage(string outcome, double durationMilliseconds)
    {
        var tags = new TagList { { CourseEventConsumerMeterConstants.Outcome, outcome } };
        _messages.Add(1, tags);
        _messageDuration.Record(durationMilliseconds, tags);
    }

    public void RecordFailure(string operation, string failureType)
        => _failures.Add(1, new TagList
        {
            { CourseEventConsumerMeterConstants.Operation, operation },
            { CourseEventConsumerMeterConstants.ErrorType, failureType }
        });

    public void RecordDuplicate(string status)
        => _duplicates.Add(1, new TagList { { CourseEventConsumerMeterConstants.Status, status } });

    public void RecordOrchestrationStarted(string name)
        => _orchestrations.Add(1, new TagList
        {
            { CourseEventConsumerMeterConstants.OrchestrationName, name }
        });

    public void RecordActivity(string name, string outcome, double durationMilliseconds)
    {
        var tags = new TagList
        {
            { CourseEventConsumerMeterConstants.ActivityName, name },
            { CourseEventConsumerMeterConstants.Outcome, outcome }
        };
        _activities.Add(1, tags);
        _activityDuration.Record(durationMilliseconds, tags);
    }

    public void RecordFunctionStarted(string name)
    {
        var tags = new TagList { { CourseEventConsumerMeterConstants.FunctionName, name } };
        _activeFunctions.Add(1, tags);
    }

    public void RecordFunctionCompleted(string name, double durationMilliseconds, string outcome)
    {
        var tags = new TagList
        {
            { CourseEventConsumerMeterConstants.FunctionName, name },
            { CourseEventConsumerMeterConstants.Outcome, outcome }
        };
        _activeFunctions.Add(-1, new TagList
        {
            { CourseEventConsumerMeterConstants.FunctionName, name }
        });
        _functionExecutions.Add(1, tags);
        _functionDuration.Record(durationMilliseconds, tags);
    }
}
