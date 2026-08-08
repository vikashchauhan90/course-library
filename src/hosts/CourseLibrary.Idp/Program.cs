using CourseLibrary.Idp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite("Data Source=course-library-idp.db");
    options.UseOpenIddict();
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
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
               .SetTokenEndpointUris("/connect/token")
               .SetUserinfoEndpointUris("/connect/userinfo");

        options.AllowClientCredentialsFlow();
        options.AllowPasswordFlow();
        options.AllowRefreshTokenFlow();

        options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.OfflineAccess, "api");

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

builder.Services.AddScoped<IdentityDataSeeder>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();
    await seeder.InitializeAsync();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/connect/token", async (HttpContext context) =>
{
    var openIddictRequest = context.GetOpenIddictServerRequest();
    if (openIddictRequest is null)
    {
        return Results.BadRequest();
    }

    if (openIddictRequest.IsPasswordGrantType())
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
        var signInManager = context.RequestServices.GetRequiredService<SignInManager<IdentityUser>>();
        var user = await userManager.FindByNameAsync(openIddictRequest.Username);
        if (user is null)
        {
            return Results.BadRequest();
        }

        if (!await signInManager.CheckPasswordSignInAsync(user, openIddictRequest.Password, lockoutOnFailure: false).ConfigureAwait(false))
        {
            return Results.BadRequest();
        }

        var claims = new List<Claim>
        {
            new Claim(Claims.Subject, user.Id),
            new Claim(Claims.Name, user.UserName ?? string.Empty),
            new Claim(Claims.Email, user.Email ?? string.Empty),
        };

        var claimsIdentity = new ClaimsIdentity(claims,
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            Claims.Name,
            Claims.Role);

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        claimsPrincipal.SetScopes(openIddictRequest.GetScopes());
        claimsPrincipal.SetResources("resource_server");

        return Results.Ok(claimsPrincipal);
    }

    return Results.BadRequest();
});

app.Run();
