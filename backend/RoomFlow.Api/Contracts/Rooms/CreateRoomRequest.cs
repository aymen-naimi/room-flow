namespace RoomFlow.Api.Contracts.Rooms;

public record CreateRoomRequest(string Name, int Capacity, string Location);
