namespace RoomFlow.Api.Contracts.Users;

public record CreateUserRequest(string Email, string Password, string FirstName, string LastName);
