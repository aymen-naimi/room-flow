using Microsoft.EntityFrameworkCore;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Infrastructure.Persistence;

public sealed class RefreshTokenStore : IRefreshTokenStore
{
    private readonly RoomFlowDbContext _dbContext;

    public RefreshTokenStore(RoomFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        _dbContext.RefreshTokens.Add(token);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<RefreshToken?> GetActiveByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _dbContext.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash
                    && token.RevokedAt == null
                    && token.ExpiresAt > DateTimeOffset.UtcNow,
                cancellationToken);

    public async Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        token.RevokedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
