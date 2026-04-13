using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BBScoreboard.Domain.Enums;
using BBScoreboard.Infrastructure.Identity;

namespace BBScoreboard.Web.Pages.Manage;

[Authorize(Roles = "Admin")]
public class UserEntryModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public UserEntryModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<int>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public ApplicationUser? AppUser { get; set; }
    public bool IsNew => AppUser == null || AppUser.Id == 0;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id.HasValue)
        {
            AppUser = await _userManager.FindByIdAsync(id.Value.ToString());
            if (AppUser == null) return NotFound();
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id, string email, string? firstName, string? lastName, int access, string? password)
    {
        if (id.HasValue && id > 0)
        {
            var user = await _userManager.FindByIdAsync(id.Value.ToString());
            if (user == null) return NotFound();
            user.Email = email;
            user.UserName = email;
            user.FirstName = firstName ?? "";
            user.LastName = lastName ?? "";
            user.Access = (AccessType)access;
            await _userManager.UpdateAsync(user);
            await SyncRolesAsync(user);
        }
        else
        {
            var user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                FirstName = firstName ?? "",
                LastName = lastName ?? "",
                Access = (AccessType)access,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, password!);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                AppUser = user;
                return Page();
            }
            await SyncRolesAsync(user);
        }

        return RedirectToPage("Users");
    }

    private async Task SyncRolesAsync(ApplicationUser user)
    {
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (user.Access == AccessType.Admin)
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole<int>("Admin"));
            await _userManager.AddToRoleAsync(user, "Admin");
        }
        if (user.Access == AccessType.Scorer)
        {
            if (!await _roleManager.RoleExistsAsync("Scorer"))
                await _roleManager.CreateAsync(new IdentityRole<int>("Scorer"));
            await _userManager.AddToRoleAsync(user, "Scorer");
        }
    }
}
