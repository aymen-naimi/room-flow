namespace RoomFlow.Api;

public sealed class AuthCookieOptions
{
    public const string SectionName = "AuthCookie";

    public bool Secure { get; set; }

    public string SameSite { get; set; } = "Lax";
}
