using CourseLibrary.Idp;
using CourseLibrary.Idp.Domain.Entities;
using CourseLibrary.Idp.Domain.Abstractions;
using CourseLibrary.Idp.Infrastructure.Identity;
using CourseLibrary.Idp.Application.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Host=localhost;Port=5432;Database=course_library_idp;Username=course_library;Password=change_me";

builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseOpenIddict();
});

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationIdentityDbContext>());

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationIdentityDbContext>();
    })
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("/connect/authorize")
            .SetTokenEndpointUris("/connect/token");

        options.AllowClientCredentialsFlow();
        options.AllowPasswordFlow();
        options.AllowRefreshTokenFlow();

        options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.OfflineAccess, "api");

        options.AddEphemeralEncryptionKey()
            .AddEphemeralSigningKey();

        options.AcceptAnonymousClients();
        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddScoped<IIdentityProvisioningService, IdentityProvisioningService>();

builder.Services.AddAuthentication()
    .AddCookie();

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddHealthChecks();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IIdentityProvisioningService>();
    await seeder.EnsureSeedDataAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("DefaultCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapPost("/connect/token", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var grantType = form["grant_type"].ToString();
    if (string.Equals(grantType, "password", StringComparison.OrdinalIgnoreCase))
    {
        var username = form["username"].ToString();
        var password = form["password"].ToString();
        var scope = form["scope"].ToString();

        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
        var user = await userManager.FindByNameAsync(username);
        if (user is null)
        {
            return Results.BadRequest();
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false).ConfigureAwait(false);
        if (!signInResult.Succeeded)
        {
            return Results.BadRequest();
        }

        var claims = new List<Claim>
        {
            new Claim(Claims.Subject, user.Id),
            new Claim(Claims.Name, user.UserName ?? string.Empty),
            new Claim(Claims.Email, user.Email ?? string.Empty),
        };

        if (!string.IsNullOrEmpty(scope))
        {
            foreach (var s in scope.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                claims.Add(new Claim("scope", s));
            }
        }

        claims.Add(new Claim("aud", "resource_server"));

        var claimsIdentity = new ClaimsIdentity(claims,
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            Claims.Name,
            Claims.Role);

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await context.SignInAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, claimsPrincipal);
        return Results.Ok();
    }

    return Results.BadRequest();
});

app.Run();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("DefaultCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapPost("/connect/token", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var grantType = form["grant_type"].ToString();
    if (string.Equals(grantType, "password", StringComparison.OrdinalIgnoreCase))
    {
        var username = form["username"].ToString();
        var password = form["password"].ToString();
        var scope = form["scope"].ToString();

        var userManager = context.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
        var signInManager = context.RequestServices.GetRequiredService<SignInManager<IdentityUser>>();
        var user = await userManager.FindByNameAsync(username);
        if (user is null)
        {
            return Results.BadRequest();
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false).ConfigureAwait(false);
        if (!signInResult.Succeeded)
        {
            return Results.BadRequest();
        }

        var claims = new List<Claim>
        {
            new Claim(Claims.Subject, user.Id),
            new Claim(Claims.Name, user.UserName ?? string.Empty),
            new Claim(Claims.Email, user.Email ?? string.Empty),
        };

        // add scopes as scope claims so token contains them
        if (!string.IsNullOrEmpty(scope))
        {
            foreach (var s in scope.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                claims.Add(new Claim("scope", s));
            }
        }

        // audience/resource
        claims.Add(new Claim("aud", "resource_server"));

        var claimsIdentity = new ClaimsIdentity(claims,
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            Claims.Name,
            Claims.Role);

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await context.SignInAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, claimsPrincipal);
        return Results.Ok();
    }

    return Results.BadRequest();
});

app.Run();
