namespace RoomFlow.Api;

public static class RefreshTokenCookie
{
    public const string Name = "rf";
    public const string Path = "/api/auth";

    public static void Set(HttpResponse response, string refreshToken, TimeSpan lifetime, AuthCookieOptions options)
        => response.Cookies.Append(Name, refreshToken, Build(options, lifetime));

    public static void Clear(HttpResponse response, AuthCookieOptions options)
        => response.Cookies.Delete(Name, Build(options, TimeSpan.Zero));

    public static bool TryRead(HttpRequest request, out string refreshToken)
    {
        if (request.Cookies.TryGetValue(Name, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            refreshToken = value;
            return true;
        }

        refreshToken = string.Empty;
        return false;
    }

    internal static CookieOptions Build(AuthCookieOptions options, TimeSpan lifetime)
    {
        var sameSite = ParseSameSite(options.SameSite);
        var cookie = new CookieOptions
        {
            HttpOnly = true,
            Secure = options.Secure || sameSite == SameSiteMode.None,
            SameSite = sameSite,
            Path = Path,
            IsEssential = true
        };

        if (lifetime > TimeSpan.Zero)
        {
            cookie.MaxAge = lifetime;
            cookie.Expires = DateTimeOffset.UtcNow.Add(lifetime);
        }

        if (sameSite == SameSiteMode.None)
        {
            cookie.Extensions.Add("Partitioned");
        }

        return cookie;
    }

    private static SameSiteMode ParseSameSite(string? value)
        => Enum.TryParse<SameSiteMode>(value, ignoreCase: true, out var mode)
            ? mode
            : SameSiteMode.Lax;
}
