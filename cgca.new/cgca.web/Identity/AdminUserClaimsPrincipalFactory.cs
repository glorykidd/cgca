using cgca.web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace cgca.web.Identity;

public class AdminUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AdminUser, IdentityRole>
{
    public AdminUserClaimsPrincipalFactory(
        UserManager<AdminUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AdminUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (!string.IsNullOrWhiteSpace(user.DisplayName))
            identity.AddClaim(new Claim("display_name", user.DisplayName));
        return identity;
    }
}
