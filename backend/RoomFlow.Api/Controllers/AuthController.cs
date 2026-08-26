using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RoomFlow.Api.Contracts.Auth;
using RoomFlow.Api.Contracts.Users;
using RoomFlow.Application.Features.Auth.Commands.Login;
using RoomFlow.Application.Features.Auth.Commands.Refresh;

namespace RoomFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        return Ok(ToResponse(result));
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResponse>> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RefreshCommand(request.RefreshToken), cancellationToken);
        return Ok(ToResponse(result));
    }

    private static LoginResponse ToResponse(LoginResult result)
        => new(
            result.AccessToken,
            result.RefreshToken,
            new UserResponse(result.User.Id, result.User.Email, result.User.FirstName, result.User.LastName));
}
