using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using RoomFlow.Application.Abstractions.Security;

namespace RoomFlow.Infrastructure.Security;

public sealed class RefreshTokenFactory : IRefreshTokenFactory
{
    private readonly JwtOptions _options;

    public RefreshTokenFactory(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public IssuedRefreshToken Create()
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return new IssuedRefreshToken(raw, Hash(raw), DateTimeOffset.UtcNow.AddHours(_options.RefreshTokenExpirationHours));
    }

    public string Hash(string raw)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}
