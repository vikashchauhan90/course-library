using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "CourseLibrary.App.Session";
    options.AccessDeniedPath = "/home/access-denied";
})
.AddOpenIdConnect(options =>
{
    options.Authority = builder.Configuration["OpenId:Authority"]
        ?? throw new InvalidOperationException("OpenId:Authority is missing.");
    options.ClientId = builder.Configuration["OpenId:ClientId"]
        ?? throw new InvalidOperationException("OpenId:ClientId is missing.");
    options.ClientSecret = builder.Configuration["OpenId:ClientSecret"];
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.UsePkce = true;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.MapInboundClaims = false;
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("roles");
    options.Scope.Add("course-library-api");
    options.CallbackPath = "/signin-oidc";
    options.SignedOutCallbackPath = "/signout-callback-oidc";
    options.RequireHttpsMetadata = true;
});

builder.Services.AddHttpClient<CourseGatewayClient>((services, client) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(
        configuration["Gateway:BaseUrl"]
        ?? throw new InvalidOperationException("Gateway:BaseUrl is missing."));
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public sealed class CourseGatewayClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
{
    public async Task<CourseDetails?> GetCourseAsync(string courseId, string partitionKey, CancellationToken cancellationToken)
    {
        var accessToken = await httpContextAccessor.HttpContext!
            .GetTokenAsync("access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
            return null;

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}/{Uri.EscapeDataString(partitionKey)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CourseDetails>(cancellationToken);
    }
}

public sealed record CourseDetails(
    string? Id,
    string? Title,
    string? Description,
    string? AuthorId);
