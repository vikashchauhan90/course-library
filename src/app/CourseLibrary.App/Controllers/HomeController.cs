using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.App.Controllers;

public sealed class HomeController(CourseGatewayClient courseGatewayClient) : Controller
{
    [Authorize]
    public IActionResult Index() => View();

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Course(string? courseId, string? partitionKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(courseId) || string.IsNullOrWhiteSpace(partitionKey))
            return View(new CourseLookupViewModel());

        var course = await courseGatewayClient.GetCourseAsync(courseId, partitionKey, cancellationToken);
        return View(new CourseLookupViewModel
        {
            CourseId = courseId,
            PartitionKey = partitionKey,
            Course = course,
            NotFound = course is null
        });
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    [AllowAnonymous]
    public IActionResult Error() => View();

    public IActionResult Login() => Challenge(OpenIdConnectDefaults.AuthenticationScheme);

    public IActionResult Logout() => SignOut(
        new[] { "Cookies", OpenIdConnectDefaults.AuthenticationScheme });
}

public sealed class CourseLookupViewModel
{
    public string? CourseId { get; init; }
    public string? PartitionKey { get; init; }
    public CourseDetails? Course { get; init; }
    public bool NotFound { get; init; }
}
