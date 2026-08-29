using RoomFlow.Domain.Enums;

namespace RoomFlow.Application.Abstractions.Data;

public record UserDto(Guid Id, string Email, string FirstName, string LastName, UserRole Role);
