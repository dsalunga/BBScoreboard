using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BBScoreboard.Web.Pages.Manage;

[Authorize(Roles = "Admin")]
public class RefereesModel : PageModel
{
    public void OnGet()
    {
    }
}
