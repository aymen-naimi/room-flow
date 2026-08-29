namespace RoomFlow.Application.Exceptions;

public sealed class EmailAlreadyTakenException : Exception
{
    public EmailAlreadyTakenException(string email)
        : base($"A user with email '{email}' already exists.")
    {
    }
}
