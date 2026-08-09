using CourseLibrary.Idp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using static OpenIddict.Abstractions.OpenIddictConstants;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=course-library-idp.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(connectionString);
    options.UseOpenIddict();
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
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
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>();
    })
        .AddServer(options =>
        {
           options.SetAuthorizationEndpointUris("/connect/authorize")
               .SetTokenEndpointUris("/connect/token");

         options.AllowClientCredentialsFlow();
         options.AllowPasswordFlow();
         options.AllowRefreshTokenFlow();

         options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.OfflineAccess, "api");

         // Use ephemeral keys for signing/encryption in development. Replace with persisted keys/certificates in production.
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

builder.Services.AddAuthentication()
    .AddCookie();

builder.Services.AddAuthorization();

// CORS: allow the gateway origin to call this IdP
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

builder.Services.AddScoped<IdentityDataSeeder>();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// OpenTelemetry disabled in build step to avoid missing extension issues; add back in when packages and usings are aligned.

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();
    await seeder.InitializeAsync();
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
