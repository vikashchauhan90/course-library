namespace CourseLibrary.Idp.Models;

public sealed record ErrorPageViewModel(
    string Title,
    string Summary,
    string Detail,
    string ActionText)
{
    public int StatusCode { get; init; }
    public string TraceId { get; init; } = string.Empty;
    public string ActionUrl { get; init; } = "/";
}