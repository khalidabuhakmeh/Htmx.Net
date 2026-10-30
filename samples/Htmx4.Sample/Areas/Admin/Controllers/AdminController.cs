using Microsoft.AspNetCore.Mvc;

namespace Htmx4.Sample.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin")]
public class AdminController : Controller
{
    [HttpGet("ping")]
    public IActionResult Ping() =>
        Htmx4.Sample.Controllers.DemoController.Frag("<span data-testid=\"url-result\">area Admin ping</span>");
}
