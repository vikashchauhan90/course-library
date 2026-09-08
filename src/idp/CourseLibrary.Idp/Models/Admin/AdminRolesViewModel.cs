namespace CourseLibrary.Idp.Models.Admin;

public sealed record AdminRolesViewModel(
    IReadOnlyList<AdminRoleItem> Roles,
    IReadOnlyList<AdminPermissionItem> Permissions);