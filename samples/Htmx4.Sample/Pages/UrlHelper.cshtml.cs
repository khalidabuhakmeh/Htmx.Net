using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Htmx4.Sample.Controllers;

namespace Htmx4.Sample.Pages;

public class UrlHelperModel : PageModel
{
    public void OnGet() { }

    public IActionResult OnGetFragment() =>
        DemoController.Frag($"<span data-testid=\"url-result\">page handler Fragment path={DemoController.H(Request.Path)}{DemoController.H(Request.QueryString.Value)}</span>");

    public IActionResult OnPostPost() =>
        DemoController.Frag("<span data-testid=\"url-result\">page handler Post ok</span>");
}
