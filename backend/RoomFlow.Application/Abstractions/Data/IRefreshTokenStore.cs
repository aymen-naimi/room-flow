using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Abstractions.Data;

public interface IRefreshTokenStore
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetActiveByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken = default);
}
