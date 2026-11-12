using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.CustomerManagement.Controllers;

public sealed class HomeController : Controller
{
    [AllowAnonymous]
    [HttpGet("/home/error")]
    public IActionResult Error() => Problem("The customer management service could not complete the request.");

    [AllowAnonymous]
    [HttpGet("/home/access-denied")]
    public IActionResult AccessDenied() => View();

    [AllowAnonymous]
    [HttpGet("/home/signed-out")]
    public IActionResult SignedOut() => View();

    [HttpPost("/account/logout")]
    [ValidateAntiForgeryToken]
    public IActionResult Logout(string? returnUrl) =>
        SignOut(
            new AuthenticationProperties
            {
                RedirectUri = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? returnUrl
                    : "/"
            },
            "Cookies",
            OpenIdConnectDefaults.AuthenticationScheme);
}
