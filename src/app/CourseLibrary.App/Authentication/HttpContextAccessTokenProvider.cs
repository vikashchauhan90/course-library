using CourseLibrary.Client.Security;
using Microsoft.AspNetCore.Authentication;

namespace CourseLibrary.App.Authentication;

public sealed class HttpContextAccessTokenProvider(IHttpContextAccessor httpContextAccessor) : IAccessTokenProvider
{
    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        return httpContext is null
            ? Task.FromResult<string?>(null)
            : httpContext.GetTokenAsync("access_token");
    }
}
