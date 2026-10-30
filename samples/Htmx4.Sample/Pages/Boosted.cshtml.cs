using Htmx;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Htmx4.Sample.Pages;

public class BoostedModel : PageModel
{
    public string RequestType { get; private set; } = "";
    public string PostResult { get; private set; } = "(none)";

    public void OnGet() => RequestType = Request.IsHtmx(out var h) ? h!.RequestType.ToString() : "n/a";

    public void OnPost()
    {
        RequestType = Request.IsHtmx(out var h) ? h!.RequestType.ToString() : "n/a";
        PostResult = $"boosted={Request.IsHtmxBoosted()} token-validated=true";
    }
}
