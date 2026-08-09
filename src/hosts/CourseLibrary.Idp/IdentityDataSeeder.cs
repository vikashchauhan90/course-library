using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CourseLibrary.Idp;

internal sealed class IdentityDataSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IConfiguration _configuration;

    public IdentityDataSeeder(
        ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager,
        IOpenIddictApplicationManager applicationManager,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _applicationManager = applicationManager;
        _configuration = configuration;
    }

    public async Task InitializeAsync()
    {
        // Use migrations in production; fall back to EnsureCreated in trimmed scenarios.
        await _dbContext.Database.MigrateAsync();

        await SeedApplicationsAsync();
        await SeedUsersAsync();
    }

    private async Task SeedApplicationsAsync()
    {
        const string gatewayClientId = "course-library-gateway";

        if (await _applicationManager.FindByClientIdAsync(gatewayClientId) is null)
        {
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

            await _applicationManager.CreateAsync(descriptor);
        }
    }

    private async Task SeedUsersAsync()
    {
        const string userName = "alice";
        const string userEmail = "alice@courselibrary.local";
        const string password = "Pass123$";

        if (await _userManager.FindByNameAsync(userName) is null)
        {
            var user = new IdentityUser(userName)
            {
                Email = userEmail,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create seed user: {string.Join(';', result.Errors.Select(e => e.Description))}");
            }

            await _userManager.AddClaimsAsync(user, new[]
            {
                new Claim(Claims.Subject, user.Id),
                new Claim(Claims.Name, userName),
                new Claim(Claims.Email, userEmail)
            });
        }
    }
}
