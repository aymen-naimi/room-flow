namespace RoomFlow.Application.Abstractions.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenExpirationMinutes { get; set; } = 15;

    public int RefreshTokenExpirationHours { get; set; } = 24;
}
