namespace RoomFlow.Api.Contracts.Rooms;

public record RoomResponse(Guid Id, string Name, int Capacity, string Location, DateTimeOffset CreatedAt);
