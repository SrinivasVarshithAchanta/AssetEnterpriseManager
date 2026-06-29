using System.Security.Claims;

namespace EnterpriseAssetManager.Services;

// Small helpers so controllers can read the signed in user's id and name
// from the authentication cookie without repeating claim lookups everywhere.
public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out int id) ? id : 0;
    }

    public static string GetDisplayName(this ClaimsPrincipal principal)
    {
        return principal?.FindFirstValue(ClaimTypes.Name) ?? "User";
    }

    public static string GetRole(this ClaimsPrincipal principal)
    {
        return principal?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }
}
