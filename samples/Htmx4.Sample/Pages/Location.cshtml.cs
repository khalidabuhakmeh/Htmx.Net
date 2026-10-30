using Htmx;
using Htmx4.Sample.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Htmx4.Sample.Pages;

public class LocationModel : PageModel
{
    public void OnGet() { }

    public IActionResult OnGetGo()
    {
        Response.Htmx(h => h.Location(new HtmxLocation
        {
            Path = "/Location?handler=Landing",
            Target = "#loc-dest",
            Swap = HtmxSwap.InnerHtml,
            Select = "#loc-incoming",
            SelectOob = "#loc-oob-incoming:#loc-oob",
            Values = new Dictionary<string, object> { ["from"] = "hx-location", ["n"] = 1 },
            Headers = new Dictionary<string, string> { ["X-Demo"] = "loc" },
            Transition = true
        }));
        return DemoController.Frag("<span>navigating</span>");
    }

    public IActionResult OnGetHeaderOnly()
    {
        Response.Htmx(h => h.Location(new HtmxLocation { Path = "/Location?handler=Landing", Source = "#loc-start", Event = "click", Target = "#loc-dest" }));
        var value = Response.Headers[HtmxResponseHeaders.Keys.Location].ToString();
        Response.Headers.Remove(HtmxResponseHeaders.Keys.Location);
        return DemoController.Frag(DemoController.H(value));
    }

    public IActionResult OnGetGoString()
    {
        Response.Htmx(h => h.Location("/Location?handler=Landing"));
        return DemoController.Frag("<span>navigating (string)</span>");
    }

    public IActionResult OnGetLanding(string? from, int? n)
    {
        Request.IsHtmx(out var h);
        return DemoController.Frag(
            "<div id=\"loc-incoming\" data-testid=\"loc-incoming\">landed"
            + $" from=[{DemoController.H(from)}] n=[{n}] x-demo=[{DemoController.H(Request.Headers["X-Demo"])}]"
            + $" boosted={Request.IsHtmxBoosted()}</div>"
            + "<div id=\"loc-oob-incoming\" data-testid=\"loc-oob-incoming\">oob content</div>"
            + "<div id=\"loc-skipped\" data-testid=\"loc-skipped\">skipped by select</div>");
    }
}
