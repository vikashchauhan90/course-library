#if false
using CourseLibrary.Idp.Domain.Entities;
using CourseLibrary.Idp.Domain.Abstractions;
using CourseLibrary.Idp.Infrastructure.Persistence;
using CourseLibrary.Idp.Infrastructure.Persistence.Interceptors;
using CourseLibrary.Idp.Application.Services;
using CourseLibrary.Idp.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Host=localhost;Port=5432;Database=course_library_idp;Username=course_library;Password=change_me";

builder.Services.AddScoped<AuditEntityInterceptor>();
builder.Services.AddScoped<SecurityEntityInterceptor>();

builder.Services.AddDbContext<ApplicationDbContext>((provider, options) =>
{
    options.UseNpgsql(connectionString);
    options.UseOpenIddict();
    options.AddInterceptors(
        provider.GetRequiredService<AuditEntityInterceptor>(),
        provider.GetRequiredService<SecurityEntityInterceptor>());
});

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

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

builder.Services.AddScoped<IIdentityProvisioningService, CourseLibrary.Idp.Infrastructure.Services.IdentityProvisioningService>();

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
#endif

using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;
using CourseLibrary.Idp.Application.Services;
using CourseLibrary.Idp.Domain.Abstractions;
using CourseLibrary.Idp.Domain.Entities;
using CourseLibrary.Idp.Infrastructure.Persistence;
using CourseLibrary.Idp.Infrastructure.Persistence.Interceptors;
using CourseLibrary.Idp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be supplied through configuration or a secret store.");
}
var issuer = builder.Configuration["OpenId:Issuer"]
    ?? throw new InvalidOperationException("OpenId:Issuer is required.");

if (!Uri.TryCreate(issuer, UriKind.Absolute, out var issuerUri) || issuerUri.Scheme != Uri.UriSchemeHttps)
{
    throw new InvalidOperationException("OpenId:Issuer must be an absolute HTTPS URL.");
}

builder.Services.AddScoped<AuditEntityInterceptor>();
builder.Services.AddScoped<SecurityEntityInterceptor>();
builder.Services.AddDbContext<ApplicationDbContext>((provider, options) =>
{
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));
    options.UseSnakeCaseNamingConvention();
    options.UseOpenIddict();
    options.AddInterceptors(provider.GetRequiredService<AuditEntityInterceptor>(),
        provider.GetRequiredService<SecurityEntityInterceptor>());
});
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.User.RequireUniqueEmail = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.SlidingExpiration = false;
});

builder.Services.AddOpenIddict()
    .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>())
    .AddServer(options =>
    {
        options.SetIssuer(issuerUri);
        options.SetTokenEndpointUris("/connect/token");
        options.AllowClientCredentialsFlow();
        options.RegisterScopes("course-library-api");
        options.DisableAccessTokenEncryption();
        if (builder.Environment.IsDevelopment())
        {
            options.AddEphemeralEncryptionKey().AddEphemeralSigningKey();
        }
        else
        {
            var certificatePath = builder.Configuration["OpenId:Certificates:Path"]
                ?? throw new InvalidOperationException("OpenId:Certificates:Path is required outside development.");
            var certificatePassword = builder.Configuration["OpenId:Certificates:Password"]
                ?? throw new InvalidOperationException("OpenId:Certificates:Password is required outside development.");
            var certificate = X509CertificateLoader.LoadPkcs12FromFile(
                certificatePath,
                certificatePassword);
            options.AddEncryptionCertificate(certificate).AddSigningCertificate(certificate);
        }
        options.UseAspNetCore().EnableTokenEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddScoped<IIdentityProvisioningService, IdentityProvisioningService>();
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("token", context =>
    {
        var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("Idp", policy =>
{
    if (allowedOrigins.Length > 0)
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));
builder.Services.AddHealthChecks();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();
if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<IIdentityProvisioningService>().EnsureSeedDataAsync();
}

if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("Idp");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapHealthChecks("/health/live").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();

app.MapPost("/connect/token", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync(context.RequestAborted);
    if (!string.Equals(form["grant_type"], "client_credentials", StringComparison.Ordinal))
        return Results.BadRequest(new { error = Errors.UnsupportedGrantType });

    var clientId = form["client_id"].ToString()
        ?? throw new InvalidOperationException("Client ID is required for client credentials.");
    var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
        Claims.Name, Claims.Role);
    identity.SetClaim(Claims.Subject, $"{clientId}@clients");
    identity.SetClaim(Claims.AuthorizedParty, clientId);
    identity.SetScopes(form["scope"].ToString()
        .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    identity.SetResources("course-library-api");
    identity.SetDestinations(claim => claim.Type switch
    {
        Claims.Subject or Claims.AuthorizedParty => [Destinations.AccessToken],
        _ => []
    });

    await context.SignInAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
        new ClaimsPrincipal(identity));
    return Results.Empty;
}).AllowAnonymous().RequireRateLimiting("token");

app.Run();
