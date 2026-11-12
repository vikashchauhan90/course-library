using CourseLibrary.CustomerManagement.Data;
using CourseLibrary.CustomerManagement.Infrastructure;
using MailKit.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<CustomerManagementDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("CustomerManagement");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("ConnectionStrings:CustomerManagement must be configured.");

    options.UseNpgsql(connectionString, postgres =>
            postgres.MigrationsHistoryTable("__EFMigrationsHistory", "customer_management"))
        .UseSnakeCaseNamingConvention();
});

builder.Services.AddOptions<SmtpEmailOptions>()
    .Bind(builder.Configuration.GetSection(SmtpEmailOptions.SectionName))
    .Validate(options => options.IsValid,
        "Email:Smtp requires a host, a valid sender address, a valid port, and either both or neither SMTP credentials.");
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "__Host-CourseLibrary.CustomerManagement";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.AccessDeniedPath = "/home/access-denied";
})
.AddOpenIdConnect(options =>
{
    options.Authority = builder.Configuration["OpenId:Authority"]
        ?? throw new InvalidOperationException("OpenId:Authority must be configured.");
    options.ClientId = builder.Configuration["OpenId:ClientId"]
        ?? throw new InvalidOperationException("OpenId:ClientId must be configured.");
    options.ClientSecret = builder.Configuration["OpenId:ClientSecret"]
        ?? throw new InvalidOperationException("OpenId:ClientSecret must be configured.");
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.UsePkce = true;
    options.SaveTokens = true;
    options.MapInboundClaims = false;
    options.GetClaimsFromUserInfoEndpoint = false;
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.CallbackPath = "/signin-oidc";
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
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
app.MapControllerRoute("default", "{controller=Organizations}/{action=Index}/{id?}");

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CustomerManagementDbContext>();
    if (builder.Configuration.GetValue("Database:ApplyMigrationsOnStartup", true))
        await dbContext.Database.MigrateAsync();
    if (builder.Configuration.GetValue("Database:SeedDevelopmentCatalog", true))
        await DevelopmentCatalogSeeder.SeedAsync(dbContext);
}

app.Run();

public partial class Program;
