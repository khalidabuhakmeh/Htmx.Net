using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

public class HomeController : Controller
{
    [HttpGet, Route("home")]
    public IActionResult Index(int id)
    {
        return Content($"<h2>Hello, From Home Controller (id: {id})</h2>");
    }

    [ValidateAntiForgeryToken]
    [HttpPost, Route("home/name", Name = "named")]
    public IActionResult Named()
    {
        return Content("<h2>Hello, From Named Route</h2>");
    }
}