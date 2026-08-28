using System.Security.Claims;

namespace RoomFlow.Api;

internal static class UserClaims
{
    public static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out userId);
    }
}
