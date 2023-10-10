using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Htmx;

namespace Sample.Pages
{
    public class FormsModel : PageModel
    {
        [BindProperty]
        [Required, Display(Name = "Username", Prompt = "(Any username will do)")]
        public string? Username { get; set; }

        [BindProperty]
        [Required, Display(Name = "Password", Prompt = "(Anything but the word password)")]
        public string? Password { get; set; }

        public async Task<IActionResult> OnPost()
        {
            // simulating waiting...
            await Task.Delay(1000);

            var isValid = Password != "password";

            if (!isValid)
            {
                return Content("Username or password is wrong", "text/html");
            }
            else
            {
                return Content($"Welcome {Username}!", "text/html");
            }
        }
    }
}
