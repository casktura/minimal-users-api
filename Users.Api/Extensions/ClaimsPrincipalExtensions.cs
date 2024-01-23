using System.Security.Claims;

namespace Users.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int UserId(this ClaimsPrincipal claimsPrincipal)
    {
        return int.Parse(claimsPrincipal.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
    }
}
