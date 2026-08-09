using CourseLibrary.Idp.Domain.Abstractions;
using CourseLibrary.Idp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CourseLibrary.Idp.Application.Services;

public sealed class IdentityProvisioningService : IIdentityProvisioningService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentityProvisioningService> _logger;

    public IdentityProvisioningService(
        IApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOpenIddictApplicationManager applicationManager,
        IConfiguration configuration,
        ILogger<IdentityProvisioningService> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _applicationManager = applicationManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnsureSeedDataAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ensuring IdP schema and seed data...");

        await _dbContext.Database.MigrateAsync(cancellationToken);

        await EnsureRolesAsync(cancellationToken);
        await EnsureUsersAsync(cancellationToken);
        await EnsureApplicationsAsync(cancellationToken);
    }

    private async Task EnsureRolesAsync(CancellationToken cancellationToken)
    {
        const string adminRole = "Administrator";

        if (await _roleManager.RoleExistsAsync(adminRole).ConfigureAwait(false))
        {
            return;
        }

        var role = new ApplicationRole(adminRole)
        {
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var result = await _roleManager.CreateAsync(role).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create role: {string.Join(';', result.Errors.Select(e => e.Description))}");
        }
    }

    private async Task EnsureUsersAsync(CancellationToken cancellationToken)
    {
        const string userName = "alice";
        const string userEmail = "alice@courselibrary.local";
        const string password = "Pass123$";

        if (await _userManager.FindByNameAsync(userName).ConfigureAwait(false) is not null)
        {
            return;
        }

        var user = new ApplicationUser(userName)
        {
            Email = userEmail,
            EmailConfirmed = true,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var createResult = await _userManager.CreateAsync(user, password).ConfigureAwait(false);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create seed user: {string.Join(';', createResult.Errors.Select(e => e.Description))}");
        }

        await _userManager.AddClaimsAsync(user, new[]
        {
            new Claim(Claims.Subject, user.Id),
            new Claim(Claims.Name, user.UserName ?? string.Empty),
            new Claim(Claims.Email, user.Email ?? string.Empty),
            new Claim("role", "Administrator")
        }).ConfigureAwait(false);
    }

    private async Task EnsureApplicationsAsync(CancellationToken cancellationToken)
    {
        const string gatewayClientId = "course-library-gateway";

        if (await _applicationManager.FindByClientIdAsync(gatewayClientId).ConfigureAwait(false) is not null)
        {
            return;
        }

        var secret = _configuration["OpenId:Clients:Gateway:ClientSecret"] ?? "gateway-secret";

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = gatewayClientId,
            ClientSecret = secret,
            DisplayName = "CourseLibrary Gateway Client"
        };

        descriptor.Permissions.Add(Permissions.Endpoints.Token);
        descriptor.Permissions.Add(Permissions.GrantTypes.ClientCredentials);
        descriptor.Permissions.Add(Permissions.GrantTypes.Password);
        descriptor.Permissions.Add(Permissions.GrantTypes.RefreshToken);
        descriptor.Permissions.Add(Permissions.Prefixes.Scope + "api");
        descriptor.Permissions.Add(Permissions.Prefixes.Scope + Scopes.Email);
        descriptor.Permissions.Add(Permissions.Prefixes.Scope + Scopes.Profile);
        descriptor.Permissions.Add(Permissions.Prefixes.Scope + Scopes.OfflineAccess);

        await _applicationManager.CreateAsync(descriptor).ConfigureAwait(false);
    }
}
