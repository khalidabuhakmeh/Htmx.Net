using System.Collections.Concurrent;
using System.Text.Json;
using Htmx.TagHelpers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace Htmx4.Sample.Controllers;

/// <summary>
/// TEST-ONLY endpoints used by test/Htmx.E2ETests AntiforgeryTests. They simulate a hostile second origin
/// (localhost vs 127.0.0.1 on the same server) that records what it receives. Not a demo of recommended practice.
/// </summary>
[Route("demo/af-test")]
[IgnoreAntiforgeryToken]
public class AntiforgeryTestController : Controller
{
    private static readonly ConcurrentQueue<object> Captured = new();

    /// <summary>Permissive-CORS "attacker" endpoint: reflects origin/headers/methods, records host, method, header token and form token.</summary>
    [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS", Route = "capture")]
    public async Task<IActionResult> Capture()
    {
        var origin = Request.Headers.Origin.ToString();
        if (origin.Length > 0)
        {
            Response.Headers["Access-Control-Allow-Origin"] = origin;
            Response.Headers["Access-Control-Allow-Headers"] = Request.Headers.AccessControlRequestHeaders.ToString() is { Length: > 0 } h ? h : "*";
            Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, PATCH, DELETE, OPTIONS";
            Response.Headers["Vary"] = "Origin";
        }
        if (HttpMethods.IsOptions(Request.Method)) return NoContent();

        var headerToken = Request.Headers["RequestVerificationToken"].ToString();
        var formToken = "";
        if (Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync();
            formToken = form.Where(kv => kv.Key.Contains("RequestVerificationToken")).Select(kv => kv.Value.ToString()).FirstOrDefault() ?? "";
        }
        Captured.Enqueue(new { host = Request.Host.Value, method = Request.Method, origin, headerToken, formToken });
        return DemoController.Frag($"<span data-testid=\"x-captured\">captured by {DemoController.H(Request.Host.Value)}</span>");
    }

    [HttpGet("captured")]
    public IActionResult GetCaptured() => Json(Captured.ToArray());

    [HttpPost("reset")]
    public IActionResult Reset() { Captured.Clear(); return NoContent(); }

    /// <summary>Redirects (307 default, or ?code=308) to ?url=..., preserving method and body.</summary>
    [AcceptVerbs("POST", "PUT", "PATCH", "DELETE", "GET", Route = "redirect-to")]
    public IActionResult RedirectTo(string url, int code = 307)
    {
        Response.StatusCode = code;
        Response.Headers.Location = url;
        return new EmptyResult();
    }

    /// <summary>The non-minified antiforgery script, served separately so both variants can be tested.</summary>
    [HttpGet("antiforgery.full.js")]
    public IActionResult FullScript() => Content(HtmxSnippets.AntiforgeryJavaScript, "text/javascript");
}
