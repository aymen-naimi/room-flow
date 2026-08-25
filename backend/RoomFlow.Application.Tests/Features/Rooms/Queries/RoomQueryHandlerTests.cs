using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Features.Rooms.Queries.GetRoomById;
using RoomFlow.Application.Features.Rooms.Queries.GetRooms;
using RoomFlow.Application.Tests.Fakes;

namespace RoomFlow.Application.Tests.Features.Rooms.Queries;

public sealed class GetRoomsQueryHandlerTests
{
    [Fact]
    public async Task Handle_returns_rooms_from_store()
    {
        var store = new FakeRoomReadStore();
        var room = new RoomDto(Guid.NewGuid(), "Salle A", 8, "RDC", DateTimeOffset.UtcNow);
        store.Rooms.Add(room);
        var handler = new GetRoomsQueryHandler(store);

        var result = await handler.Handle(new GetRoomsQuery(), CancellationToken.None);

        Assert.Equal(room, Assert.Single(result));
    }
}

public sealed class GetRoomByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_returns_room_when_found()
    {
        var store = new FakeRoomReadStore();
        var room = new RoomDto(Guid.NewGuid(), "Salle A", 8, "RDC", DateTimeOffset.UtcNow);
        store.Rooms.Add(room);
        var handler = new GetRoomByIdQueryHandler(store);

        var result = await handler.Handle(new GetRoomByIdQuery(room.Id), CancellationToken.None);

        Assert.Equal(room, result);
    }

    [Fact]
    public async Task Handle_returns_null_when_missing()
    {
        var handler = new GetRoomByIdQueryHandler(new FakeRoomReadStore());

        var result = await handler.Handle(new GetRoomByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
