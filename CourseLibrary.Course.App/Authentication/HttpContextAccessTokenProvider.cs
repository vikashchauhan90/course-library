using CourseLibrary.Course.Client.Security;
using Microsoft.AspNetCore.Authentication;

namespace CourseLibrary.Course.App.Authentication;

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
