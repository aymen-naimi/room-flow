namespace RoomFlow.Application.Exceptions;

public sealed class RoomNameAlreadyTakenException : Exception
{
    public RoomNameAlreadyTakenException(string name)
        : base($"A room named '{name}' already exists.")
    {
    }
}
