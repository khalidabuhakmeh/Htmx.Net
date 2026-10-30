using Htmx4.Sample.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Htmx4.Sample.Pages;

public class PartialsModel : PageModel
{
    public void OnGet() { }

    public IActionResult OnGetOnly() => DemoController.Frag(
        "<hx-partial hx-target=\"#part-c\" hx-swap=\"outerHTML\"><div id=\"part-c\" data-testid=\"part-c\">C (partial only)</div></hx-partial>");
}
