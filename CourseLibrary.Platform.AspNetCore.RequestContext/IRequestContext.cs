namespace CourseLibrary.Platform.AspNetCore.RequestContext;

public interface IRequestContext
{
    string? TraceId { get; }
    string? TraceParent { get; }
    string? TraceState { get; }
    string? CorrelationId { get; }
    string? UserId { get; }
    string? TenantId { get; }
    string? IdempotencyKey { get; }
}