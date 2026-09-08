namespace CourseLibrary.Idp.Domain.Entities;

public sealed class RolePermission
{
    public required string RoleId { get; set; }
    public required string PermissionId { get; set; }

    public ApplicationRole Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}