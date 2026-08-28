namespace RoomFlow.Application.Abstractions.Data;

public record BookingDto(
    Guid Id,
    Guid RoomId,
    string RoomName,
    Guid UserId,
    string UserDisplayName,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
