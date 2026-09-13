using CourseLibrary.Client;
using CourseLibrary.Client.Courses;
using CourseLibrary.Models.Course;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourseLibrary.App.Controllers;

public sealed class HomeController(ICourseApiClient courseApiClient) : Controller
{
    [Authorize]
    public IActionResult Index() => View();

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Course(string? courseId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(courseId))
            return View(new CourseLookupViewModel());

        try
        {
            var course = await courseApiClient.GetCourseAsync(courseId, cancellationToken);
            return View(new CourseLookupViewModel
            {
                CourseId = courseId,
                Course = course.Data,
                NotFound = course.Data is null
            });
        }
        catch (CourseApiException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return View(new CourseLookupViewModel
            {
                CourseId = courseId,
                NotFound = true
            });
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Search(string? q, CancellationToken cancellationToken)
    {
        try
        {
            var criteria = new CourseSearchCriteria(
                q,
                AuthorId: null,
                IncludeDeleted: false,
                IncludeRetired: false);
            var page = await courseApiClient.SearchAsync(criteria, cancellationToken: cancellationToken);
            return View(new CourseSearchViewModel(q, page.Data.Items.Select(item => item.Data).ToArray()));
        }
        catch (CourseApiException exception) when (exception.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return await ReauthenticateAfterUnauthorizedAsync();
        }
        catch (CourseApiException)
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
            var userId = User.FindFirstValue("sub");
            var criteria = new CourseSearchCriteria(
                null,
                AuthorId: userId,
                IncludeDeleted: false,
                IncludeRetired: false);
            var page = await courseApiClient.SearchAsync(criteria, cancellationToken: cancellationToken);
            return View(page.Data.Items.Select(item => item.Data).ToArray());
        }
        catch (CourseApiException exception) when (exception.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
        var userId = User.FindFirstValue("sub");
        var userName = User.FindFirstValue("email");
        await courseApiClient.CreateAsync(
            new CreateCourseRequest(
                model.Title,
                model.Description,
                userId!,
                userName!),
            cancellationToken);
        return RedirectToAction(nameof(Mine));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit(string courseId, CancellationToken cancellationToken)
    {
        var course = await courseApiClient.GetCourseAsync(courseId, cancellationToken);
        if (course.Data is null) return NotFound();
        return View(new CourseFormViewModel
        {
            CourseId = course.Data.Id,
            Title = course.Data.Title,
            Description = course.Data.Description
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CourseFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        await courseApiClient.UpdateAsync(
            model.CourseId!,
            new UpdateCourseRequest(model.Title, model.Description),
            cancellationToken);
        return RedirectToAction(nameof(Mine));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string courseId, CancellationToken cancellationToken)
    {
        await courseApiClient.DeleteAsync(courseId, cancellationToken);
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
    public CourseResponse? Course { get; init; }
    public bool NotFound { get; init; }
}

public sealed record CourseSearchViewModel(
    string? Query,
    IReadOnlyList<CourseResponse> Courses,
    string? Error = null);

public sealed class CourseFormViewModel
{
    public string? CourseId { get; init; }
    public string? PartitionKey { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
