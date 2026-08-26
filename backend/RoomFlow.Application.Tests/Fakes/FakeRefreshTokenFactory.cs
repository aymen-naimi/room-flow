using RoomFlow.Application.Abstractions.Security;

namespace RoomFlow.Application.Tests.Fakes;

internal sealed class FakeRefreshTokenFactory : IRefreshTokenFactory
{
    private int _sequence;

    public IssuedRefreshToken Create()
    {
        _sequence++;
        var raw = $"refresh-raw-{_sequence}";
        return new IssuedRefreshToken(raw, Hash(raw), DateTimeOffset.UtcNow.AddHours(24));
    }

    public string Hash(string raw) => $"hash:{raw}";
}
