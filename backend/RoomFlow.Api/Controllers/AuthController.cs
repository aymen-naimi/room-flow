using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RoomFlow.Api.Contracts.Auth;
using RoomFlow.Api.Contracts.Users;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Features.Auth.Commands.Login;
using RoomFlow.Application.Features.Auth.Commands.Logout;
using RoomFlow.Application.Features.Auth.Commands.Refresh;
using RoomFlow.Application.Features.Users.Commands.CreateUser;

namespace RoomFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<UserResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);
        var user = await _sender.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ToUserResponse(user));
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        return Ok(ToResponse(result));
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<LoginResponse>> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RefreshCommand(request.RefreshToken), cancellationToken);
        return Ok(ToResponse(result));
    }

    [HttpPost("logout")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
        return NoContent();
    }

    private static LoginResponse ToResponse(LoginResult result)
        => new(
            result.AccessToken,
            result.RefreshToken,
            ToUserResponse(result.User));

    private static UserResponse ToUserResponse(UserDto user)
        => new(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString());
}
