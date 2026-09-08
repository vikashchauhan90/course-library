namespace CourseLibrary.Idp.Models.Admin;

public sealed record AdminUsersViewModel(
    IReadOnlyList<AdminUserItem> Users);