namespace CourseLibrary.Application.Abstractions.RequestContext;

public interface IRequestContext
{
    string? TraceId { get; }
    string? TraceParent { get; }
    string? TraceState { get; }
    string? CorrelationId { get; }
    string? UserId { get; }
    string? ClientId { get; }
    string? IdempotencyKey { get; }
    bool IsAuthenticated { get; }
}