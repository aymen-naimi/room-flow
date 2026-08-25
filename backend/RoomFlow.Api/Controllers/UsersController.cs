using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RoomFlow.Api.Contracts.Users;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Features.Users.Commands.CreateUser;

namespace RoomFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [EnableRateLimiting("register")]
    public async Task<ActionResult<UserResponse>> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);
        var user = await _sender.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ToResponse(user));
    }

    private static UserResponse ToResponse(UserDto user)
        => new(user.Id, user.Email, user.FirstName, user.LastName);
}
