using RoomFlow.Application.Features.Rooms.Commands.DeleteRoom;
using RoomFlow.Application.Tests.Fakes;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Tests.Features.Rooms.Commands;

public sealed class DeleteRoomCommandHandlerTests
{
    [Fact]
    public async Task Handle_removes_room_when_it_exists()
    {
        var store = new FakeRoomWriteStore();
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Salle A",
            Capacity = 8,
            Location = "RDC",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = Guid.NewGuid()
        };
        await store.AddAsync(room);
        var handler = new DeleteRoomCommandHandler(store);

        var deleted = await handler.Handle(new DeleteRoomCommand(room.Id), CancellationToken.None);

        Assert.True(deleted);
        Assert.Empty(store.Rooms);
    }

    [Fact]
    public async Task Handle_returns_false_when_room_is_missing()
    {
        var handler = new DeleteRoomCommandHandler(new FakeRoomWriteStore());

        var deleted = await handler.Handle(new DeleteRoomCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(deleted);
    }
}
