using RoomFlow.Application.Features.Users.Commands.CreateUser;
using RoomFlow.Application.Tests.Fakes;

namespace RoomFlow.Application.Tests.Features.Users.Commands;

public sealed class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_adds_user_when_email_is_available()
    {
        var store = new FakeUserWriteStore();
        var handler = new CreateUserCommandHandler(store, new FakePasswordHasher());
        var command = new CreateUserCommand("Ada@Example.com", "password1", "Ada", "Lovelace");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("ada@example.com", result.Email);
        Assert.Equal("Ada", result.FirstName);
        Assert.Equal("Lovelace", result.LastName);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Single(store.Users);
        Assert.Equal(result.Id, store.Users[0].Id);
        Assert.Equal("hashed:password1", store.Users[0].PasswordHash);
    }

    [Fact]
    public async Task Handle_throws_when_email_is_already_taken()
    {
        var store = new FakeUserWriteStore();
        var hasher = new FakePasswordHasher();
        await new CreateUserCommandHandler(store, hasher).Handle(
            new CreateUserCommand("ada@example.com", "password1", "Ada", "Lovelace"),
            CancellationToken.None);
        var handler = new CreateUserCommandHandler(store, hasher);

        var exception = await Assert.ThrowsAsync<EmailAlreadyTakenException>(
            () => handler.Handle(
                new CreateUserCommand("Ada@Example.com", "password2", "Ada", "Byron"),
                CancellationToken.None));

        Assert.Contains("ada@example.com", exception.Message, StringComparison.Ordinal);
        Assert.Single(store.Users);
    }
}
