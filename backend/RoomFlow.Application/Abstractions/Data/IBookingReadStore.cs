namespace RoomFlow.Application.Abstractions.Data;

public interface IBookingReadStore
{
    Task<IReadOnlyList<BookingDto>> GetOverlappingAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? roomId = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> HasOverlapAsync(
        Guid roomId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        CancellationToken cancellationToken = default);
}
