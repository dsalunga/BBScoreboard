using System.Text;
using BBScoreboard.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BBScoreboard.Web.Pages.Account;

[AllowAnonymous]
public class ConfirmModel(UserManager<ApplicationUser> userManager) : PageModel
{
    public string StatusMessage { get; set; } = "Email confirmation is not available.";

    public async Task OnGetAsync(string? userId, string? token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            StatusMessage = "Email confirmation link is missing or invalid.";
            return;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            StatusMessage = "Account not found.";
            return;
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            StatusMessage = "Invalid confirmation token format.";
            return;
        }

        var result = await userManager.ConfirmEmailAsync(user, decodedToken);
        StatusMessage = result.Succeeded
            ? "Email confirmed. You can now sign in."
            : "Unable to confirm email. The link may be expired or invalid.";
    }
}
