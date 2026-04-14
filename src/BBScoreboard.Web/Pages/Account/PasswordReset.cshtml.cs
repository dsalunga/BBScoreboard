using System.ComponentModel.DataAnnotations;
using System.Text;
using BBScoreboard.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BBScoreboard.Web.Pages.Account;

[AllowAnonymous]
public class PasswordResetModel(UserManager<ApplicationUser> userManager) : PageModel
{
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet(string? email, string? token)
    {
        Input.Email = email ?? string.Empty;
        Input.Token = token ?? string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            StatusMessage = "Password reset processed. If the account exists, you can now sign in with the new password.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Input.Token))
        {
            ModelState.AddModelError(string.Empty, "Reset token is required.");
            return Page();
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Token));
        }
        catch (FormatException)
        {
            ModelState.AddModelError(string.Empty, "Reset token format is invalid.");
            return Page();
        }

        var result = await userManager.ResetPasswordAsync(user, decodedToken, Input.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        StatusMessage = "Password has been reset. You can now sign in.";
        return Page();
    }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Password confirmation does not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
