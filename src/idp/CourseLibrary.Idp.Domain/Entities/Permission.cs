using CourseLibrary.Idp.Domain.Abstractions;

namespace CourseLibrary.Idp.Domain.Entities;

public sealed class Permission : IEntity<string>
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
    public required string Resource { get; set; }
    public required string Action { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}