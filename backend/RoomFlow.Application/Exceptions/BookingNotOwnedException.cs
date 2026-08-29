namespace RoomFlow.Application.Exceptions;

public sealed class BookingNotOwnedException : Exception
{
    public BookingNotOwnedException(Guid bookingId)
        : base($"Booking '{bookingId}' belongs to another user.")
    {
    }
}
