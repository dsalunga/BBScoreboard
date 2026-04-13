using BBScoreboard.Domain.Enums;
using BBScoreboard.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BBScoreboard.Web.Pages;

[Authorize]
public class IndexModel(UserManager<ApplicationUser> userManager) : PageModel
{
    public bool IsAdmin { get; set; }

    public async Task OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        IsAdmin = user?.Access == AccessType.Admin;
    }
}
