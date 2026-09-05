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
    options.GetClaimsFromUserInfoEndpoint = false;
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
    private HttpClient HttpClient => httpClient;

    private async Task<HttpRequestMessage> CreateRequestAsync(
        HttpMethod method,
        string path,
        CancellationToken cancellationToken)
    {
        var accessToken = await httpContextAccessor.HttpContext!
            .GetTokenAsync("access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new InvalidOperationException("The current session has no access token.");

        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    public async Task<CourseDetails?> GetCourseAsync(string courseId, string partitionKey, CancellationToken cancellationToken)
    {
        using var request = await CreateRequestAsync(
            HttpMethod.Get,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}/{Uri.EscapeDataString(partitionKey)}",
            cancellationToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<CourseDetails>(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseDetails>> SearchAsync(string? query, CancellationToken cancellationToken)
    {
        using var request = await CreateRequestAsync(
            HttpMethod.Get,
            $"api/v1/courses/search?q={Uri.EscapeDataString(query?.Trim() ?? string.Empty)}&pageSize=50",
            cancellationToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<CourseDetails>>(cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<CourseDetails>> GetMineAsync(CancellationToken cancellationToken)
    {
        using var request = await CreateRequestAsync(HttpMethod.Get, "api/v1/courses/mine", cancellationToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<CourseDetails>>(cancellationToken) ?? [];
    }

    public async Task<CourseDetails> CreateAsync(CreateCourseRequest requestModel, CancellationToken cancellationToken)
    {
        using var request = await CreateRequestAsync(HttpMethod.Post, "api/v1/courses/", cancellationToken);
        request.Content = JsonContent.Create(requestModel);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<CourseDetails>(cancellationToken))!;
    }

    public async Task<CourseDetails> UpdateAsync(string courseId, string partitionKey, UpdateCourseRequest requestModel, CancellationToken cancellationToken)
    {
        using var request = await CreateRequestAsync(
            HttpMethod.Put,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}/{Uri.EscapeDataString(partitionKey)}",
            cancellationToken);
        request.Content = JsonContent.Create(requestModel);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<CourseDetails>(cancellationToken))!;
    }

    public async Task DeleteAsync(string courseId, string partitionKey, CancellationToken cancellationToken)
    {
        using var request = await CreateRequestAsync(
            HttpMethod.Delete,
            $"api/v1/courses/{Uri.EscapeDataString(courseId)}/{Uri.EscapeDataString(partitionKey)}",
            cancellationToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var detail = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"Gateway returned {(int)response.StatusCode} ({response.StatusCode}). {detail}",
            inner: null,
            response.StatusCode);
    }
}

public sealed record CourseDetails(
    string? Id,
    string? Title,
    string? Description,
    string? AuthorId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateCourseRequest(string Title, string Description, string AuthorId);

public sealed record UpdateCourseRequest(string Title, string Description, string AuthorId);
