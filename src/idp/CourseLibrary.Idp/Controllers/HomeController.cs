using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.Idp.Controllers;

public sealed class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index() => View();

    [HttpGet("/home/error")]
    public IActionResult Error() => View();
}
