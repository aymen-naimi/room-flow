using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Tests.Fakes;

internal sealed class FakeRoomWriteStore : IRoomWriteStore
{
    public IList<Room> Rooms { get; } = new List<Room>();

    public Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default)
        => Task.FromResult(Rooms.Any(room => room.Name == name));

    public Task AddAsync(Room room, CancellationToken cancellationToken = default)
    {
        Rooms.Add(room);
        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = Rooms.FirstOrDefault(entity => entity.Id == id);
        if (room is null)
        {
            return Task.FromResult(false);
        }

        Rooms.Remove(room);
        return Task.FromResult(true);
    }
}

internal sealed class FakeRoomReadStore : IRoomReadStore
{
    public IList<RoomDto> Rooms { get; } = new List<RoomDto>();

    public Task<IReadOnlyList<RoomDto>> GetRoomsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<RoomDto>>(Rooms.ToList());

    public Task<RoomDto?> GetRoomByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Rooms.FirstOrDefault(room => room.Id == id));
}
