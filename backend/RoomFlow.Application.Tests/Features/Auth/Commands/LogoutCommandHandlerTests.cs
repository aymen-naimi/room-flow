using RoomFlow.Application.Features.Auth.Commands.Logout;
using RoomFlow.Application.Tests.Fakes;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Tests.Features.Auth.Commands;

public sealed class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_revokes_active_refresh_token()
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
        var handler = new LogoutCommandHandler(factory, store);

        await handler.Handle(new LogoutCommand(issued.Raw), CancellationToken.None);

        Assert.NotNull(store.Tokens[0].RevokedAt);
    }

    [Fact]
    public async Task Handle_succeeds_when_refresh_token_is_unknown()
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
        var handler = new LogoutCommandHandler(factory, store);

        await handler.Handle(new LogoutCommand("nope"), CancellationToken.None);

        Assert.Null(store.Tokens[0].RevokedAt);
    }

    [Fact]
    public async Task Handle_leaves_already_revoked_token_unchanged()
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
        var revokedAt = new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);
        store.Tokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = issued.Hash,
            ExpiresAt = issued.ExpiresAt,
            RevokedAt = revokedAt,
            User = user
        });
        var handler = new LogoutCommandHandler(factory, store);

        await handler.Handle(new LogoutCommand(issued.Raw), CancellationToken.None);

        Assert.Equal(revokedAt, store.Tokens[0].RevokedAt);
    }
}
