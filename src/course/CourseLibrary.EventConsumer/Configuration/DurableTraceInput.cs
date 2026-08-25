using CourseLibrary.EventConsumer.Configuration.Observability.Traces;

namespace CourseLibrary.EventConsumer.Configuration;

internal sealed record DurableTraceInput<T>(
    T Data,
    RequestTraceContext TraceContext);