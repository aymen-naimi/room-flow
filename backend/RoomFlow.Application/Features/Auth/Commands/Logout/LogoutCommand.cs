using MediatR;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Abstractions.Security;

namespace RoomFlow.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenFactory _refreshTokenFactory;
    private readonly IRefreshTokenStore _refreshTokenStore;

    public LogoutCommandHandler(
        IRefreshTokenFactory refreshTokenFactory,
        IRefreshTokenStore refreshTokenStore)
    {
        _refreshTokenFactory = refreshTokenFactory;
        _refreshTokenStore = refreshTokenStore;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var hash = _refreshTokenFactory.Hash(request.RefreshToken);
        var existing = await _refreshTokenStore.GetActiveByHashAsync(hash, cancellationToken);
        if (existing is null)
        {
            return;
        }

        await _refreshTokenStore.RevokeAsync(existing, cancellationToken);
    }
}
