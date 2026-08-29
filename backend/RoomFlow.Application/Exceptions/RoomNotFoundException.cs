namespace RoomFlow.Application.Exceptions;

public sealed class RoomNotFoundException : Exception
{
    public RoomNotFoundException(Guid roomId)
        : base($"Room '{roomId}' was not found.")
    {
    }
}
