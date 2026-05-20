using Microsoft.AspNetCore.Identity;

namespace cgca.web.Models;

public class AdminUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
