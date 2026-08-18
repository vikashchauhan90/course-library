using CourseLibrary.Idp.Application.Services;
using CourseLibrary.Idp.Domain.Abstractions;
using CourseLibrary.Idp.Domain.Entities;
using CourseLibrary.Idp.Infrastructure.Persistence;
using CourseLibrary.Idp.Infrastructure.Services;
using CourseLibrary.Idp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.Configure<OpenIdOptions>(builder.Configuration.GetSection(OpenIdOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
    options.UseOpenIddict();
});
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
builder.Services.AddScoped<IIdentityProvisioningService, IdentityProvisioningService>();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.Cookie.Name = "__Host-CourseLibrary.Idp";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.SlidingExpiration = true;
});
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
    options.TokenLifespan = TimeSpan.FromHours(24));

var openId = builder.Configuration.GetSection(OpenIdOptions.SectionName).Get<OpenIdOptions>()
    ?? throw new InvalidOperationException("OpenId configuration is missing.");
if (!Uri.TryCreate(openId.Issuer, UriKind.Absolute, out var issuer) || issuer.Scheme != Uri.UriSchemeHttps)
    throw new InvalidOperationException("OpenId:Issuer must be an absolute HTTPS URI.");

builder.Services.AddOpenIddict()
    .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>())
    .AddServer(options =>
    {
        options.SetIssuer(issuer);
        options.SetAuthorizationEndpointUris("connect/authorize")
            .SetEndSessionEndpointUris("connect/logout")
            .SetTokenEndpointUris("connect/token")
            .SetUserInfoEndpointUris("connect/userinfo");
        options.RegisterScopes(OpenIddictConstants.Scopes.OpenId, OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles, openId.ApiScope);
        options.AllowAuthorizationCodeFlow().AllowRefreshTokenFlow().AllowClientCredentialsFlow();
        options.RequireProofKeyForCodeExchange();
        options.DisableAccessTokenEncryption();
        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(openId.AccessTokenLifetimeMinutes));
        options.SetRefreshTokenLifetime(TimeSpan.FromDays(openId.RefreshTokenLifetimeDays));
        if (!string.IsNullOrWhiteSpace(openId.SigningCertificatePath) &&
            !string.IsNullOrWhiteSpace(openId.EncryptionCertificatePath))
        {
            var signingCertificate = X509CertificateLoader.LoadPkcs12FromFile(openId.SigningCertificatePath,
                openId.SigningCertificatePassword,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet);
            var encryptionCertificate = X509CertificateLoader.LoadPkcs12FromFile(openId.EncryptionCertificatePath,
                openId.EncryptionCertificatePassword,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet);
            if (!signingCertificate.HasPrivateKey || !encryptionCertificate.HasPrivateKey)
                throw new InvalidOperationException("OpenId signing and encryption certificates must contain private keys.");
            options.AddSigningCertificate(signingCertificate);
            options.AddEncryptionCertificate(encryptionCertificate);
        }
        else if (builder.Environment.IsDevelopment())
        {
            options.AddDevelopmentSigningCertificate();
            options.AddDevelopmentEncryptionCertificate();
        }
        else
        {
            throw new InvalidOperationException("OpenId signing and encryption certificates must be configured outside Development.");
        }
        options.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableEndSessionEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableUserInfoEndpointPassthrough()
            .EnableStatusCodePagesIntegration();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/home/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

if (builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<IIdentityProvisioningService>().EnsureSeedDataAsync();
}

app.Run();
