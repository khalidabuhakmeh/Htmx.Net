using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Sample.Pages
{
    public class ResponseModel : PageModel
    {
        [BindProperty]
        [Required, Display(Name = "Message", Prompt = "Write anything")]
        public string? Message { get; set; }

        public async Task OnPost()
        {
            // Pretend the server is doing something heavy
            await Task.Delay(500);

            Response.Htmx(headers =>
            {
                headers.WithTrigger("showMessage", new { message = Message });
            });
        }
    }
}
