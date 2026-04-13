using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BBScoreboard.Web.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel : PageModel
{
    public string? Message { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost(string email)
    {
        Message = "If an account exists for that email, contact an administrator for password reset assistance.";
        return Page();
    }
}
