namespace RoomFlow.Api.Contracts.Bookings;

public record CreateBookingRequest(Guid RoomId, DateTimeOffset StartsAt, DateTimeOffset EndsAt);
