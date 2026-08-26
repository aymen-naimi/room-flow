using MediatR;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Abstractions.Security;
using RoomFlow.Application.Features.Auth.Commands.Login;

namespace RoomFlow.Application.Features.Auth.Commands.Refresh;

public record RefreshCommand(string RefreshToken) : IRequest<LoginResult>;

public sealed class RefreshCommandHandler : IRequestHandler<RefreshCommand, LoginResult>
{
    private readonly IRefreshTokenFactory _refreshTokenFactory;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly IAccessTokenGenerator _accessTokenGenerator;

    public RefreshCommandHandler(
        IRefreshTokenFactory refreshTokenFactory,
        IRefreshTokenStore refreshTokenStore,
        IAccessTokenGenerator accessTokenGenerator)
    {
        _refreshTokenFactory = refreshTokenFactory;
        _refreshTokenStore = refreshTokenStore;
        _accessTokenGenerator = accessTokenGenerator;
    }

    public async Task<LoginResult> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var hash = _refreshTokenFactory.Hash(request.RefreshToken);
        var existing = await _refreshTokenStore.GetActiveByHashAsync(hash, cancellationToken);
        if (existing is null)
        {
            throw new InvalidCredentialsException();
        }

        await _refreshTokenStore.RevokeAsync(existing, cancellationToken);
        return await LoginCommandHandler.IssueSessionAsync(
            existing.User,
            _accessTokenGenerator,
            _refreshTokenFactory,
            _refreshTokenStore,
            cancellationToken);
    }
}
