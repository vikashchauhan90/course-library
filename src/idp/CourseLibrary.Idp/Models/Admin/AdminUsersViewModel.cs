namespace CourseLibrary.Idp.Models.Admin;

public sealed record AdminUsersViewModel(
    IReadOnlyList<AdminUserItem> Users,
    string Status,
    int Page,
    int PageSize,
    int TotalCount,
    int ActiveCount,
    int InactiveCount);