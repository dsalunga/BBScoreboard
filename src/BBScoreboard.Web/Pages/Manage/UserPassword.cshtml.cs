using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBScoreboard.Infrastructure.Identity;

namespace BBScoreboard.Web.Pages.Manage;

[Authorize(Roles = "Admin")]
public class UserPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    public UserPasswordModel(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public ApplicationUser? AppUser { get; set; }
    public string? Message { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        AppUser = await _userManager.FindByIdAsync(id.ToString());
        return AppUser == null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, string newPassword, string confirmPassword)
    {
        AppUser = await _userManager.FindByIdAsync(id.ToString());
        if (AppUser == null) return NotFound();

        if (newPassword != confirmPassword)
        {
            Message = "Passwords do not match.";
            return Page();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(AppUser);
        var result = await _userManager.ResetPasswordAsync(AppUser, token, newPassword);
        if (!result.Succeeded)
        {
            Message = string.Join("; ", result.Errors.Select(e => e.Description));
            return Page();
        }

        Message = "Password changed successfully.";
        return Page();
    }
}
