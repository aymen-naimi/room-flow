using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Tests.Fakes;

internal sealed class FakeRefreshTokenStore : IRefreshTokenStore
{
    public IList<RefreshToken> Tokens { get; } = new List<RefreshToken>();

    public Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        Tokens.Add(token);
        return Task.CompletedTask;
    }

    public Task<RefreshToken?> GetActiveByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => Task.FromResult(Tokens.FirstOrDefault(token =>
            token.TokenHash == tokenHash
            && token.RevokedAt is null
            && token.ExpiresAt > DateTimeOffset.UtcNow));

    public Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        token.RevokedAt = DateTimeOffset.UtcNow;
        return Task.CompletedTask;
    }
}
