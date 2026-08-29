using MediatR;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Abstractions.Security;
using RoomFlow.Application.Exceptions;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Features.Auth.Commands.Login;

public record LoginResult(UserDto User, string AccessToken, string RefreshToken);

public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private static string? DummyPasswordHash;

    private readonly IUserReadStore _readStore;
    private readonly IUserWriteStore _writeStore;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IRefreshTokenFactory _refreshTokenFactory;
    private readonly IRefreshTokenStore _refreshTokenStore;

    public LoginCommandHandler(
        IUserReadStore readStore,
        IUserWriteStore writeStore,
        IPasswordHasher passwordHasher,
        IAccessTokenGenerator accessTokenGenerator,
        IRefreshTokenFactory refreshTokenFactory,
        IRefreshTokenStore refreshTokenStore)
    {
        _readStore = readStore;
        _writeStore = writeStore;
        _passwordHasher = passwordHasher;
        _accessTokenGenerator = accessTokenGenerator;
        _refreshTokenFactory = refreshTokenFactory;
        _refreshTokenStore = refreshTokenStore;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _readStore.GetByEmailAsync(email, cancellationToken);
        var storedHash = user?.PasswordHash ?? (DummyPasswordHash ??= _passwordHasher.Hash("__roomflow_dummy__"));
        var passwordMatches = _passwordHasher.Verify(request.Password, storedHash);

        if (user is null || !passwordMatches)
        {
            throw new InvalidCredentialsException();
        }

        var lastSignIn = DateTimeOffset.UtcNow;
        await _writeStore.UpdateLastSignInAsync(user.Id, lastSignIn, cancellationToken);
        user.LastSignIn = lastSignIn;

        return await IssueSessionAsync(user, cancellationToken);
    }

    internal static async Task<LoginResult> IssueSessionAsync(
        User user,
        IAccessTokenGenerator accessTokenGenerator,
        IRefreshTokenFactory refreshTokenFactory,
        IRefreshTokenStore refreshTokenStore,
        CancellationToken cancellationToken)
    {
        var refresh = refreshTokenFactory.Create();
        await refreshTokenStore.AddAsync(
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = refresh.Hash,
                ExpiresAt = refresh.ExpiresAt
            },
            cancellationToken);

        var userDto = new UserDto(user.Id, user.Email, user.FirstName, user.LastName);
        var accessToken = accessTokenGenerator.Create(user.Id, user.Email);
        return new LoginResult(userDto, accessToken, refresh.Raw);
    }

    private Task<LoginResult> IssueSessionAsync(User user, CancellationToken cancellationToken)
        => IssueSessionAsync(
            user,
            _accessTokenGenerator,
            _refreshTokenFactory,
            _refreshTokenStore,
            cancellationToken);
}
