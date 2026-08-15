using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using System.Security.Claims;
using Microsoft.Data.Sqlite;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CourseLibrary.Idp;

internal sealed class IdentityDataSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager,
        IOpenIddictApplicationManager applicationManager,
        IConfiguration configuration,
        ILogger<IdentityDataSeeder> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _applicationManager = applicationManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Applying migrations for IdP database...");
            await _dbContext.Database.MigrateAsync();
            _logger.LogInformation("Database migrations applied.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Applying migrations failed — falling back to EnsureCreated.");
            await _dbContext.Database.EnsureCreatedAsync();
            _logger.LogInformation("Database ensured created.");
        }

        try
        {
            await SeedApplicationsAsync();
            await SeedUsersAsync();
        }
        catch (SqliteException ex) when (ex.Message.Contains("no such table", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(ex, "Detected missing table during seeding. Recreating database and retrying seeding.");
            await _dbContext.Database.EnsureDeletedAsync();

            try
            {
                await _dbContext.Database.MigrateAsync();
            }
            catch (Exception migEx)
            {
                _logger.LogWarning(migEx, "Migrate failed on retry — falling back to EnsureCreated.");
                await _dbContext.Database.EnsureCreatedAsync();
            }

            // Retry seeding once after recreating database.
            await SeedApplicationsAsync();
            await SeedUsersAsync();
        }
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
