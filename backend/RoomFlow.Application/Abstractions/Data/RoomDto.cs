namespace RoomFlow.Application.Abstractions.Data;

public record RoomDto(Guid Id, string Name, int Capacity, string Location, DateTimeOffset CreatedAt);
