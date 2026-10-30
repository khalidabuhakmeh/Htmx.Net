using Htmx4.Sample.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Htmx4.Sample.Pages;

[IgnoreAntiforgeryToken]
public class ErrorSwapsModel : PageModel
{
    public void OnGet() { }

    public IActionResult OnPostValidate(string? name) =>
        string.IsNullOrWhiteSpace(name)
            ? DemoController.Frag("<div data-testid=\"err-form-error\" class=\"error\">Name is required (422)</div>", 422)
            : DemoController.Frag($"<div data-testid=\"err-form-ok\">Hello {DemoController.H(name)}</div>");

    public IActionResult OnDeleteDelete(string? item)
    {
        var found = Request.HasFormContentType ? Request.Form["item"].ToString() : Request.Query["item"].ToString();
        var v = string.IsNullOrEmpty(found) ? (item ?? "") : found;
        return DemoController.Frag($"<span data-testid=\"del-result\">item=[{DemoController.H(v)}]</span>");
    }
}
