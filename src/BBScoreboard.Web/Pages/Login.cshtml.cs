using System.ComponentModel.DataAnnotations;
using BBScoreboard.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BBScoreboard.Web.Pages;

public class LoginModel(SignInManager<ApplicationUser> signInManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string? action)
    {
        if (string.Equals(action, "Logoff", StringComparison.OrdinalIgnoreCase))
        {
            await signInManager.SignOutAsync();
            return RedirectToPage("/Login");
        }

        if (signInManager.IsSignedIn(User))
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.Succeeded)
            return RedirectToPage("/Index");

        if (result.IsLockedOut)
        {
            return RedirectToPage("/Account/AccountLockedOut");
        }

        ErrorMessage = "Invalid login attempt.";
        return Page();
    }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
