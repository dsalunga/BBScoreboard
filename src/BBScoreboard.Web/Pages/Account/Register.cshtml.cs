using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBScoreboard.Domain.Enums;
using BBScoreboard.Infrastructure.Identity;

namespace BBScoreboard.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public List<string> Errors { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string email, string? firstName, string? lastName, string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            Errors.Add("Passwords do not match.");
            return Page();
        }

        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            FirstName = firstName ?? "",
            LastName = lastName ?? "",
            Access = AccessType.ReadOnly,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            Errors.AddRange(result.Errors.Select(e => e.Description));
            return Page();
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToPage("/Index");
    }
}
