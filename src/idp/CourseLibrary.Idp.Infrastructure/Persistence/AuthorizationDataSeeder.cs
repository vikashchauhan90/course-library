using CourseLibrary.Idp.Domain.Authorization;
using CourseLibrary.Idp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Idp.Infrastructure.Persistence;

public static class AuthorizationDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        var permissions = await dbContext.Permissions.ToDictionaryAsync(
            permission => permission.Id,
            StringComparer.OrdinalIgnoreCase);

        foreach (var definition in PermissionCatalog.Definitions)
        {
            if (permissions.ContainsKey(definition.Code))
                continue;

            var permission = new Permission
            {
                Id = definition.Code,
                DisplayName = definition.DisplayName,
                Resource = definition.Resource,
                Action = definition.Action
            };
            dbContext.Permissions.Add(permission);
            permissions.Add(permission.Id, permission);
        }

        await dbContext.SaveChangesAsync();

        await EnsureRoleAsync(roleManager, PermissionCatalog.AdministratorRole);
        await EnsureRoleAsync(roleManager, "Viewer");
        await EnsureRoleAsync(roleManager, "Creator");

        var existingAssignments = await dbContext.RolePermissions
            .Select(assignment => new { assignment.RoleId, assignment.PermissionId })
            .ToListAsync();
        var assignmentKeys = existingAssignments
            .Select(assignment => $"{assignment.RoleId}:{assignment.PermissionId}")
            .ToHashSet(StringComparer.Ordinal);

        foreach (var role in await roleManager.Roles.ToListAsync())
        {
            var roleClaims = await roleManager.GetClaimsAsync(role);
            var permissionCodes = roleClaims
                .Where(claim => claim.Type.Equals("permission", StringComparison.OrdinalIgnoreCase))
                .Select(claim => claim.Value)
                .Where(permission => PermissionCatalog.TryNormalize(permission, out _))
                .Select(permission => permission.Trim().ToLowerInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (role.Name?.Equals(
                    PermissionCatalog.AdministratorRole,
                    StringComparison.OrdinalIgnoreCase) == true)
            {
                permissionCodes.UnionWith(PermissionCatalog.Definitions.Select(definition => definition.Code));
            }
            else if (role.Name?.Equals("Viewer", StringComparison.OrdinalIgnoreCase) == true)
            {
                permissionCodes.UnionWith(
                    PermissionCatalog.Definitions
                        .Where(definition => definition.Action == "read")
                        .Select(definition => definition.Code));
            }
            else if (role.Name?.Equals("Creator", StringComparison.OrdinalIgnoreCase) == true)
            {
                permissionCodes.UnionWith(
                    PermissionCatalog.Definitions
                        .Where(definition => definition.Action is "read" or "write")
                        .Select(definition => definition.Code));
            }

            foreach (var permissionCode in permissionCodes)
            {
                if (!permissions.TryGetValue(permissionCode, out var permission))
                    continue;

                var key = $"{role.Id}:{permission.Id}";
                if (assignmentKeys.Add(key))
                {
                    dbContext.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id
                    });
                }
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureRoleAsync(
        RoleManager<ApplicationRole> roleManager,
        string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
            return;

        var result = await roleManager.CreateAsync(new ApplicationRole
        {
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant(),
            CreatedAt = DateTimeOffset.UtcNow
        });

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not seed the {roleName} role: "
                + string.Join(", ", result.Errors.Select(error => error.Description)));
        }
    }
}