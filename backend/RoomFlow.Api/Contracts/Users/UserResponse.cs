namespace RoomFlow.Api.Contracts.Users;

public record UserResponse(Guid Id, string Email, string FirstName, string LastName);
