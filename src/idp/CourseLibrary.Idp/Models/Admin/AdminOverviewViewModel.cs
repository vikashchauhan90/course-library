namespace CourseLibrary.Idp.Models.Admin;

public sealed record AdminOverviewViewModel(
    int UserCount,
    int RoleCount,
    int ClientCount,
    int ScopeCount);