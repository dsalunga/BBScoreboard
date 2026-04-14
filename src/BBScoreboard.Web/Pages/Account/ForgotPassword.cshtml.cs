using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using BBScoreboard.Infrastructure.Identity;
using System.Text;

namespace BBScoreboard.Web.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel(UserManager<ApplicationUser> userManager) : PageModel
{
    public string? Message { get; set; }
    public string? ResetLink { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string email)
    {
        Message = "If an account exists for that email, password reset guidance is shown below.";
        if (string.IsNullOrWhiteSpace(email))
        {
            return Page();
        }

        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user != null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            ResetLink = Url.Page("/Account/PasswordReset", null, new { email = user.Email, token = encodedToken }, Request.Scheme);
        }

        return Page();
    }
}
