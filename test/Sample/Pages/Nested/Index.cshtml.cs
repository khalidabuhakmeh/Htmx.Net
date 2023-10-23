using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sample.Pages.Nested;

public class IndexModel : PageModel
{
    public IndexModel(ILogger<IndexModel> logger)
    {
    }

    public void OnGet()
    {
    }

    public IActionResult OnPostSnippet()
    {
        return Content("<h2>Hello, World!</h2>", "text/html");
    }
}