using System.ComponentModel.DataAnnotations;
using BBScoreboard.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BBScoreboard.Web.Pages.Account;

[Authorize]
public class ManageModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : PageModel
{
    public string? StatusMessage { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool HasPassword { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                Email = User.Identity?.Name ?? "Authenticated User";
                StatusMessage = "Account profile is unavailable in the current context.";
                HasPassword = false;
                return Page();
            }

            return Challenge();
        }

        Email = user.Email ?? user.UserName ?? string.Empty;
        HasPassword = await userManager.HasPasswordAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                Email = User.Identity?.Name ?? "Authenticated User";
                StatusMessage = "Account profile is unavailable in the current context.";
                HasPassword = false;
                return Page();
            }

            return Challenge();
        }

        Email = user.Email ?? user.UserName ?? string.Empty;
        HasPassword = await userManager.HasPasswordAsync(user);

        if (!ModelState.IsValid) return Page();

        IdentityResult result;
        if (HasPassword)
        {
            result = await userManager.ChangePasswordAsync(user, Input.CurrentPassword ?? string.Empty, Input.NewPassword);
        }
        else
        {
            result = await userManager.AddPasswordAsync(user, Input.NewPassword);
        }

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        await signInManager.RefreshSignInAsync(user);
        StatusMessage = "Password updated.";

        Input = new InputModel();
        HasPassword = await userManager.HasPasswordAsync(user);
        return Page();
    }

    public class InputModel
    {
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Password confirmation does not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
