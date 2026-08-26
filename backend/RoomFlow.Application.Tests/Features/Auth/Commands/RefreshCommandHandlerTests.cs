using RoomFlow.Application.Features.Auth.Commands.Login;
using RoomFlow.Application.Features.Auth.Commands.Refresh;
using RoomFlow.Application.Tests.Fakes;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Tests.Features.Auth.Commands;

public sealed class RefreshCommandHandlerTests
{
    [Fact]
    public async Task Handle_rotates_refresh_token_when_active()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "ada@example.com",
            PasswordHash = "hashed:password1",
            FirstName = "Ada",
            LastName = "Lovelace"
        };
        var factory = new FakeRefreshTokenFactory();
        var store = new FakeRefreshTokenStore();
        var issued = factory.Create();
        store.Tokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = issued.Hash,
            ExpiresAt = issued.ExpiresAt,
            User = user
        });
        var handler = new RefreshCommandHandler(factory, store, new FakeAccessTokenGenerator());

        var result = await handler.Handle(new RefreshCommand(issued.Raw), CancellationToken.None);

        Assert.Equal("ada@example.com", result.User.Email);
        Assert.Equal("refresh-raw-2", result.RefreshToken);
        Assert.NotNull(store.Tokens[0].RevokedAt);
        Assert.Equal(2, store.Tokens.Count);
    }

    [Fact]
    public async Task Handle_throws_when_refresh_token_is_unknown()
    {
        var handler = new RefreshCommandHandler(
            new FakeRefreshTokenFactory(),
            new FakeRefreshTokenStore(),
            new FakeAccessTokenGenerator());

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => handler.Handle(new RefreshCommand("nope"), CancellationToken.None));
    }
}
