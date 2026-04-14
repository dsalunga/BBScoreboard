using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BBScoreboard.Web.Pages.Account;

public class LoginModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Login");
}
