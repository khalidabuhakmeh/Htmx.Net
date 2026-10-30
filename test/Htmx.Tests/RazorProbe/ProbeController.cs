using Microsoft.AspNetCore.Mvc;

namespace Htmx.Tests.RazorProbe;

public class ProbeController : Controller
{
    public IActionResult Show() => View();

    [HttpGet("/named/{id?}", Name = "named")]
    public IActionResult Named(int? id) => Content("named");
}
