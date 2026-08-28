namespace RoomFlow.Api.Contracts.Bookings;

public record BookingResponse(
    Guid Id,
    Guid RoomId,
    string RoomName,
    Guid UserId,
    string UserDisplayName,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
