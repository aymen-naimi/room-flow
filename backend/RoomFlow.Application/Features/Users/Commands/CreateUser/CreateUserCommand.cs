using MediatR;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Abstractions.Security;
using RoomFlow.Application.Exceptions;
using RoomFlow.Domain.Entities;
using RoomFlow.Domain.Enums;

namespace RoomFlow.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(string Email, string Password, string FirstName, string LastName)
    : IRequest<UserDto>;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserWriteStore _writeStore;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(IUserWriteStore writeStore, IPasswordHasher passwordHasher)
    {
        _writeStore = writeStore;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _writeStore.ExistsWithEmailAsync(email, cancellationToken))
        {
            throw new EmailAlreadyTakenException(email);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = UserRole.User,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _writeStore.AddAsync(user, cancellationToken);

        return new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Role);
    }
}
