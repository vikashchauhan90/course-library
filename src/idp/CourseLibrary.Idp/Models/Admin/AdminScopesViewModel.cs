namespace CourseLibrary.Idp.Models.Admin;

public sealed record AdminScopesViewModel(
    IReadOnlyList<AdminScopeItem> Scopes);