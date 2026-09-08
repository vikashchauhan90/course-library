using Microsoft.AspNetCore.Mvc;
using CourseLibrary.Idp.Models;

namespace CourseLibrary.Idp.Controllers;

public sealed class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index() => View();

    [HttpGet("/home/error")]
    public IActionResult Error()
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        return View("Error", CreateErrorPage(500));
    }

    [HttpGet("/home/status/{statusCode:int}")]
    public IActionResult Status(int statusCode)
    {
        if (statusCode is < 400 or > 599)
            statusCode = StatusCodes.Status500InternalServerError;

        Response.StatusCode = statusCode;
        return View("Error", CreateErrorPage(statusCode));
    }

    private ErrorPageViewModel CreateErrorPage(int statusCode)
    {
        ErrorPageViewModel page = statusCode switch
        {
            StatusCodes.Status400BadRequest => new("Bad request", "The request could not be understood.", "Check the information and try again.", "Try again"),
            StatusCodes.Status401Unauthorized => new("Sign-in required", "You need to sign in to continue.", "Your session may have expired or this page may require an account.", "Sign in"),
            StatusCodes.Status403Forbidden => new("Access denied", "You do not have permission to view this page.", "Ask an administrator for access or return to a page you can use.", "Go home"),
            StatusCodes.Status404NotFound => new("Page not found", "We could not find that page.", "The link may be outdated or the address may have been entered incorrectly.", "Go home"),
            StatusCodes.Status408RequestTimeout => new("Request timed out", "The server took too long to respond.", "Try the request again in a moment.", "Try again"),
            StatusCodes.Status429TooManyRequests => new("Too many requests", "Please wait before trying again.", "The service is protecting itself from a burst of traffic.", "Try again"),
            StatusCodes.Status503ServiceUnavailable => new("Service unavailable", "The service is temporarily unavailable.", "Try again shortly.", "Try again"),
            _ => new("Something went wrong", "We could not complete that request.", "Please try again or return to the home page.", "Go home")
        };

        return page with
        {
            StatusCode = statusCode,
            TraceId = HttpContext.TraceIdentifier,
            ActionUrl = page.ActionText == "Sign in"
                ? Url.Action("Login", "Account") ?? "/account/login"
                : Url.Action("Index", "Home") ?? "/"
        };
    }
}
