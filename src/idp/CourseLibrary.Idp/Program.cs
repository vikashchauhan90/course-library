using CourseLibrary.Idp;
using CourseLibrary.Idp.Domain.Entities;
using CourseLibrary.Idp.Infrastructure.Persistence;
using CourseLibrary.Idp.Infrastructure.Persistence.Interceptors;
using CourseLibrary.Idp.Abstractions;
using CourseLibrary.Idp.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.Configure<OpenIdOptions>(builder.Configuration.GetSection(OpenIdOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");

builder.Services.AddScoped<IInterceptor, AuditEntityInterceptor>();
builder.Services.AddScoped<IInterceptor, ConnectionLoggingInterceptor>();
builder.Services.AddScoped<IInterceptor, QueryTimingInterceptor>();
builder.Services.AddScoped<IInterceptor, SecurityEntityInterceptor>();
builder.Services.AddScoped<IInterceptor, TransactionLoggingInterceptor>();

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseNpgsql(
        connectionString,
        npgsql =>
        npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
    options.UseSnakeCaseNamingConvention();
    options.AddInterceptors(sp.GetServices<IInterceptor>());
}, optionsLifetime: ServiceLifetime.Scoped);

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

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddScoped<IAuthorizationHandler, AdminMfaHandler>();
builder.Services.AddAuthorizationBuilder().AddPolicy("AdminMfa", policy =>
    policy.AddRequirements(new AdminMfaRequirement()));

var externalProviders = builder.Configuration.GetSection("ExternalAuthentication");

var openId = builder.Configuration.GetSection(OpenIdOptions.SectionName).Get<OpenIdOptions>()
    ?? throw new InvalidOperationException("OpenId configuration is missing.");
if (!Uri.TryCreate(openId.Issuer, UriKind.Absolute, out var issuer) || issuer.Scheme != Uri.UriSchemeHttps)
    throw new InvalidOperationException("OpenId:Issuer must be an absolute HTTPS URI.");

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        // Configure OpenIddict to use the default entities with a custom key type.
        options.UseEntityFrameworkCore()
               .UseDbContext<ApplicationDbContext>()
              .ReplaceDefaultEntities<
                OpenIddictApplication,
                OpenIddictAuthorization,
                OpenIddictScope,
                OpenIddictToken,
                Guid>();
    })
    .AddClient(options =>
    {
        options.AllowAuthorizationCodeFlow();

        if (builder.Environment.IsDevelopment())
        {
            options.AddDevelopmentSigningCertificate();
            options.AddDevelopmentEncryptionCertificate();
        }
        else if (!string.IsNullOrWhiteSpace(openId.SigningCertificatePath) &&
                 !string.IsNullOrWhiteSpace(openId.EncryptionCertificatePath))
        {
            var clientSigningCertificate = X509CertificateLoader.LoadPkcs12FromFile(
                openId.SigningCertificatePath,
                openId.SigningCertificatePassword,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet);
            var clientEncryptionCertificate = X509CertificateLoader.LoadPkcs12FromFile(
                openId.EncryptionCertificatePath,
                openId.EncryptionCertificatePassword,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet);
            if (!clientSigningCertificate.HasPrivateKey)
                throw new InvalidOperationException("OpenId:SigningCertificatePath must contain a private key.");
            if (!clientEncryptionCertificate.HasPrivateKey)
                throw new InvalidOperationException("OpenId:EncryptionCertificatePath must contain a private key.");
            options.AddSigningCertificate(clientSigningCertificate);
            options.AddEncryptionCertificate(clientEncryptionCertificate);
        }
        else
        {
            throw new InvalidOperationException("OpenId signing and encryption certificates must be configured outside Development.");
        }

        options.UseWebProviders()
           .AddMicrosoft(options =>
           {
               var configuration =
                   externalProviders.GetSection("Microsoft");

               options.SetClientId(
                   configuration["ClientId"]
                   ?? throw new InvalidOperationException(
                       "ExternalAuthentication:Microsoft:ClientId is missing."));

               options.SetClientSecret(
                   configuration["ClientSecret"]
                   ?? throw new InvalidOperationException(
                       "ExternalAuthentication:Microsoft:ClientSecret is missing."));

               options.SetRedirectUri(
                   "callback/login/microsoft");
           })
           .AddFacebook(options =>
           {
               var configuration =
                   externalProviders.GetSection("Facebook");

               options.SetClientId(
                   configuration["ClientId"]
                   ?? throw new InvalidOperationException(
                       "ExternalAuthentication:Facebook:ClientId is missing."));

               options.SetClientSecret(
                   configuration["ClientSecret"]
                   ?? throw new InvalidOperationException(
                       "ExternalAuthentication:Facebook:ClientSecret is missing."));

               options.SetRedirectUri(
                   "callback/login/facebook");
           })
           .AddTwitter(options =>
           {
               var configuration =
                   externalProviders.GetSection("Twitter");

               options.SetClientId(
                   configuration["ClientId"]
                   ?? throw new InvalidOperationException(
                       "ExternalAuthentication:Twitter:ClientId is missing."));

               options.SetClientSecret(
                   configuration["ClientSecret"]
                   ?? throw new InvalidOperationException(
                       "ExternalAuthentication:Twitter:ClientSecret is missing."));

               options.SetRedirectUri(
                   "callback/login/twitter");
           });


        options.UseDataProtection()
            .PreferDefaultStateTokenFormat();

        options.UseSystemNetHttp();

        options.UseAspNetCore()
            .EnableRedirectionEndpointPassthrough();
    })
    .AddServer(options =>
    {
        options.SetIssuer(issuer);

        options.SetAuthorizationEndpointUris("connect/authorize")
            .SetEndSessionEndpointUris("connect/logout")
            .SetTokenEndpointUris("connect/token")
            .SetRevocationEndpointUris("connect/revoke")
            .SetUserInfoEndpointUris("connect/userinfo");

        options.RegisterScopes(
         OpenIddictConstants.Scopes.OpenId,
         OpenIddictConstants.Scopes.Profile,
         OpenIddictConstants.Scopes.Email,
         OpenIddictConstants.Scopes.Roles,
         openId.ApiScope);

        options.AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow()
            .AllowClientCredentialsFlow();

        options.RequireProofKeyForCodeExchange();

        options.DisableAccessTokenEncryption();

        options.SetAccessTokenLifetime(
              TimeSpan.FromMinutes(openId.AccessTokenLifetimeMinutes));

        options.SetRefreshTokenLifetime(
            TimeSpan.FromDays(openId.RefreshTokenLifetimeDays));

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

        options.UseDataProtection()
            .PreferDefaultAccessTokenFormat()
            .PreferDefaultAuthorizationCodeFormat()
            .PreferDefaultDeviceCodeFormat()
            .PreferDefaultRefreshTokenFormat()
            .PreferDefaultUserCodeFormat();
    })
    .AddValidation(options =>
    {
        options.UseDataProtection();
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

if (app.Environment.IsDevelopment())
    await DevelopmentDataSeeder.SeedAsync(app.Services, app.Configuration);

app.Run();
