namespace CourseLibrary.Idp.Models;

public sealed class ConsentViewModel
{
    public string ClientName { get; init; } = string.Empty;
    public IReadOnlyList<string> Scopes { get; init; } = [];
}