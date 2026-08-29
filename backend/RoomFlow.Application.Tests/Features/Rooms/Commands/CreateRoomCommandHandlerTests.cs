using RoomFlow.Application.Exceptions;
using RoomFlow.Application.Features.Rooms.Commands.CreateRoom;
using RoomFlow.Application.Tests.Fakes;

namespace RoomFlow.Application.Tests.Features.Rooms.Commands;

public sealed class CreateRoomCommandHandlerTests
{
    [Fact]
    public async Task Handle_adds_room_when_name_is_available()
    {
        var store = new FakeRoomWriteStore();
        var handler = new CreateRoomCommandHandler(store);
        var createdByUserId = Guid.NewGuid();
        var command = new CreateRoomCommand("Salle A", 8, "1er étage", createdByUserId);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Salle A", result.Name);
        Assert.Equal(8, result.Capacity);
        Assert.Equal("1er étage", result.Location);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEqual(default, result.CreatedAt);
        Assert.Single(store.Rooms);
        Assert.Equal(result.Id, store.Rooms[0].Id);
        Assert.Equal(createdByUserId, store.Rooms[0].CreatedByUserId);
    }

    [Fact]
    public async Task Handle_throws_when_name_is_already_taken()
    {
        var store = new FakeRoomWriteStore();
        var createdByUserId = Guid.NewGuid();
        await new CreateRoomCommandHandler(store).Handle(
            new CreateRoomCommand("Salle A", 8, "RDC", createdByUserId),
            CancellationToken.None);
        var handler = new CreateRoomCommandHandler(store);

        var exception = await Assert.ThrowsAsync<RoomNameAlreadyTakenException>(
            () => handler.Handle(
                new CreateRoomCommand("Salle A", 4, "RDC", createdByUserId),
                CancellationToken.None));

        Assert.Contains("Salle A", exception.Message, StringComparison.Ordinal);
        Assert.Single(store.Rooms);
    }
}
