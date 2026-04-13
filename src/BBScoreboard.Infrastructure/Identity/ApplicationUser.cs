using Microsoft.AspNetCore.Identity;
using BBScoreboard.Domain.Enums;

namespace BBScoreboard.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public AccessType Access { get; set; } = AccessType.ReadOnly;
}
