namespace RoomFlow.Application.Exceptions;

public sealed class BookingOverlapException : Exception
{
    public BookingOverlapException(Guid roomId)
        : base($"The room '{roomId}' is already booked for that time range.")
    {
    }
}
