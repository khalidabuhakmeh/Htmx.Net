using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Sample.Pages
{
    [ValidateAntiForgeryToken]
    public class HxRequestsModel : PageModel
    {
        public IActionResult OnPost()
        {
            // list of headers
            var headers = Request.Headers.ToList();
            var html = "<pre>" + JsonSerializer.Serialize(headers, new JsonSerializerOptions { WriteIndented = true }) + "</pre>";

            return Content(html, "text/html");
        }
    }
}
