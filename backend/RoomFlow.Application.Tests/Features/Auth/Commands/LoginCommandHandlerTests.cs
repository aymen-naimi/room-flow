using RoomFlow.Application.Exceptions;
using RoomFlow.Application.Features.Auth.Commands.Login;
using RoomFlow.Application.Tests.Fakes;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Tests.Features.Auth.Commands;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_returns_user_and_tokens_when_credentials_match()
    {
        var store = new FakeUserReadStore();
        var refreshStore = new FakeRefreshTokenStore();
        store.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "ada@example.com",
            PasswordHash = "hashed:password1",
            FirstName = "Ada",
            LastName = "Lovelace"
        });
        var handler = new LoginCommandHandler(
            store,
            new FakePasswordHasher(),
            new FakeAccessTokenGenerator(),
            new FakeRefreshTokenFactory(),
            refreshStore);

        var result = await handler.Handle(new LoginCommand("Ada@Example.com", "password1"), CancellationToken.None);

        Assert.Equal("ada@example.com", result.User.Email);
        Assert.StartsWith("token:", result.AccessToken);
        Assert.Equal("refresh-raw-1", result.RefreshToken);
        Assert.Single(refreshStore.Tokens);
    }

    [Fact]
    public async Task Handle_throws_when_password_is_wrong()
    {
        var store = new FakeUserReadStore();
        store.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "ada@example.com",
            PasswordHash = "hashed:password1",
            FirstName = "Ada",
            LastName = "Lovelace"
        });
        var handler = new LoginCommandHandler(
            store,
            new FakePasswordHasher(),
            new FakeAccessTokenGenerator(),
            new FakeRefreshTokenFactory(),
            new FakeRefreshTokenStore());

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => handler.Handle(new LoginCommand("ada@example.com", "wrong"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_throws_when_email_is_unknown()
    {
        var handler = new LoginCommandHandler(
            new FakeUserReadStore(),
            new FakePasswordHasher(),
            new FakeAccessTokenGenerator(),
            new FakeRefreshTokenFactory(),
            new FakeRefreshTokenStore());

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => handler.Handle(new LoginCommand("nobody@example.com", "password1"), CancellationToken.None));
    }
}
