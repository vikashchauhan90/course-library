using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Search(string? q, CancellationToken cancellationToken)
    {
        try
        {
            var courses = await courseGatewayClient.SearchAsync(q, cancellationToken);
            return View(new CourseSearchViewModel(q, courses));
        }
        catch (HttpRequestException exception) when (exception.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return await ReauthenticateAfterUnauthorizedAsync();
        }
        catch (HttpRequestException)
        {
            return View(new CourseSearchViewModel(q, [], "The gateway denied the request."));
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Mine(CancellationToken cancellationToken)
    {
        try
        {
            var courses = await courseGatewayClient.GetMineAsync(cancellationToken);
            return View(courses);
        }
        catch (HttpRequestException exception) when (exception.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return await ReauthenticateAfterUnauthorizedAsync();
        }
    }

    [Authorize]
    [HttpGet]
    public IActionResult Create() => View(new CourseFormViewModel());

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var authorId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(authorId)) return Challenge();
        await courseGatewayClient.CreateAsync(
            new CreateCourseRequest(model.Title, model.Description, authorId), cancellationToken);
        return RedirectToAction(nameof(Mine));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit(string courseId, string partitionKey, CancellationToken cancellationToken)
    {
        var course = await courseGatewayClient.GetCourseAsync(courseId, partitionKey, cancellationToken);
        if (course is null) return NotFound();
        return View(new CourseFormViewModel
        {
            CourseId = course.Id,
            PartitionKey = partitionKey,
            Title = course.Title ?? string.Empty,
            Description = course.Description ?? string.Empty
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CourseFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var authorId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(authorId)) return Challenge();
        await courseGatewayClient.UpdateAsync(
            model.CourseId!,
            model.PartitionKey!,
            new UpdateCourseRequest(model.Title, model.Description, authorId),
            cancellationToken);
        return RedirectToAction(nameof(Mine));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string courseId, string partitionKey, CancellationToken cancellationToken)
    {
        await courseGatewayClient.DeleteAsync(courseId, partitionKey, cancellationToken);
        return RedirectToAction(nameof(Mine));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    [AllowAnonymous]
    public IActionResult Error() => View();

    public IActionResult Login() => Challenge(OpenIdConnectDefaults.AuthenticationScheme);

    public IActionResult Logout() => SignOut(
        new[] { "Cookies", OpenIdConnectDefaults.AuthenticationScheme });

    private async Task<IActionResult> ReauthenticateAfterUnauthorizedAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
    }
}

public sealed class CourseLookupViewModel
{
    public string? CourseId { get; init; }
    public string? PartitionKey { get; init; }
    public CourseDetails? Course { get; init; }
    public bool NotFound { get; init; }
}

public sealed record CourseSearchViewModel(
    string? Query,
    IReadOnlyList<CourseDetails> Courses,
    string? Error = null);

public sealed class CourseFormViewModel
{
    public string? CourseId { get; init; }
    public string? PartitionKey { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
