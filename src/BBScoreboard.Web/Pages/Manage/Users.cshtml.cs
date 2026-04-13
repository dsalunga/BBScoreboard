using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BBScoreboard.Infrastructure.Identity;

namespace BBScoreboard.Web.Pages.Manage;

[Authorize(Roles = "Admin")]
public class UsersModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersModel(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public List<ApplicationUser> UserList { get; set; } = new();
    public int CurrentUserId { get; set; }

    public async Task OnGetAsync()
    {
        UserList = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
        var currentUser = await _userManager.GetUserAsync(User);
        CurrentUserId = currentUser?.Id ?? 0;
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null)
            await _userManager.DeleteAsync(user);
        return RedirectToPage();
    }
}
