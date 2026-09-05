using CourseLibrary.Idp.Domain.Entities;
using CourseLibrary.Idp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CourseLibrary.Idp;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        if (configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        if (!configuration.GetValue<bool>("Database:SeedDevelopmentUser"))
            return;

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var applicationManager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = serviceProvider.GetRequiredService<IOpenIddictScopeManager>();

        await SeedAdministratorAsync(userManager, roleManager, configuration);
        await SeedApiScopeAsync(scopeManager, configuration);
        await SeedWebApplicationAsync(applicationManager, configuration);
    }

    private static async Task SeedAdministratorAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IConfiguration configuration)
    {
        const string roleName = "Administrator";
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var role = new ApplicationRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                CreatedAt = DateTimeOffset.UtcNow
            };
            var roleResult = await roleManager.CreateAsync(role);
            EnsureSuccess(roleResult, "administrator role");
        }

        var email = Required(configuration, "Database:DevelopmentAdminEmail");
        var password = Required(configuration, "Database:DevelopmentAdminPassword");
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = Required(configuration, "Database:DevelopmentAdminFullName"),
                CreatedAt = DateTimeOffset.UtcNow,
                LockoutEnabled = true
            };
            var userResult = await userManager.CreateAsync(user, password);
            EnsureSuccess(userResult, "development administrator");
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            var roleResult = await userManager.AddToRoleAsync(user, roleName);
            EnsureSuccess(roleResult, "development administrator role assignment");
        }
    }

    private static async Task SeedApiScopeAsync(
        IOpenIddictScopeManager scopeManager,
        IConfiguration configuration)
    {
        var scopeName = configuration["OpenId:ApiScope"] ?? "course-library-api";
        if (await scopeManager.FindByNameAsync(scopeName) is not null)
            return;

        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = scopeName,
            DisplayName = "Course Library API"
        };
        descriptor.Resources.Add(scopeName);
        await scopeManager.CreateAsync(descriptor);
    }

    private static async Task SeedWebApplicationAsync(
        IOpenIddictApplicationManager applicationManager,
        IConfiguration configuration)
    {
        var section = configuration.GetSection("OpenId:Clients:CourseLibraryApp");
        var clientId = Required(section, "ClientId");
        if (await applicationManager.FindByClientIdAsync(clientId) is not null)
            return;

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = Required(section, "ClientSecret"),
            DisplayName = section["DisplayName"] ?? "Course Library Web App"
        };

        foreach (var redirectUri in section.GetSection("RedirectUris").Get<string[]>() ?? [])
        {
            if (Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri))
                descriptor.RedirectUris.Add(uri);
        }

        descriptor.Permissions.Add(Permissions.Endpoints.Authorization);
        descriptor.Permissions.Add(Permissions.Endpoints.Token);
        descriptor.Permissions.Add(Permissions.GrantTypes.AuthorizationCode);
        descriptor.Permissions.Add(Permissions.ResponseTypes.Code);
        descriptor.Permissions.Add(Permissions.Prefixes.Scope + Scopes.OpenId);
        descriptor.Permissions.Add(Permissions.Prefixes.Scope + Scopes.Profile);
        descriptor.Permissions.Add(Permissions.Prefixes.Scope + Scopes.Email);
        descriptor.Permissions.Add(Permissions.Prefixes.Scope + Scopes.Roles);
        descriptor.Permissions.Add(Permissions.Prefixes.Scope + (configuration["OpenId:ApiScope"] ?? "course-library-api"));
        descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);

        await applicationManager.CreateAsync(descriptor);
    }

    private static string Required(IConfiguration configuration, string key) =>
        configuration[key] ?? throw new InvalidOperationException($"{key} must be configured when development seeding is enabled.");

    private static string Required(IConfigurationSection section, string key) =>
        section[key] ?? throw new InvalidOperationException($"{section.Path}:{key} must be configured when development seeding is enabled.");

    private static void EnsureSuccess(IdentityResult result, string operation)
    {
        if (result.Succeeded)
            return;

        throw new InvalidOperationException(
            $"Could not seed {operation}: {string.Join("; ", result.Errors.Select(error => error.Description))}");
    }
}
