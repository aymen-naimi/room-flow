namespace RoomFlow.Application.Abstractions.Data;

public interface IRoomReadStore
{
    Task<IReadOnlyList<RoomDto>> GetRoomsAsync(CancellationToken cancellationToken = default);

    Task<RoomDto?> GetRoomByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
