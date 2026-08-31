using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using RoomFlow.Api.Contracts.Auth;
using RoomFlow.Api.Contracts.Users;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Abstractions.Security;
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
    private readonly JwtOptions _jwt;
    private readonly AuthCookieOptions _cookie;

    public AuthController(
        ISender sender,
        IOptions<JwtOptions> jwt,
        IOptions<AuthCookieOptions> cookie)
    {
        _sender = sender;
        _jwt = jwt.Value;
        _cookie = cookie.Value;
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
        SetRefreshCookie(result.RefreshToken);
        return Ok(ToResponse(result));
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<LoginResponse>> Refresh(CancellationToken cancellationToken)
    {
        if (!RefreshTokenCookie.TryRead(Request, out var refreshToken))
        {
            return Unauthorized();
        }

        var result = await _sender.Send(new RefreshCommand(refreshToken), cancellationToken);
        SetRefreshCookie(result.RefreshToken);
        return Ok(ToResponse(result));
    }

    [HttpPost("logout")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (RefreshTokenCookie.TryRead(Request, out var refreshToken))
        {
            await _sender.Send(new LogoutCommand(refreshToken), cancellationToken);
        }

        RefreshTokenCookie.Clear(Response, _cookie);
        return NoContent();
    }

    private void SetRefreshCookie(string refreshToken)
        => RefreshTokenCookie.Set(
            Response,
            refreshToken,
            TimeSpan.FromHours(_jwt.RefreshTokenExpirationHours),
            _cookie);

    private static LoginResponse ToResponse(LoginResult result)
        => new(result.AccessToken, ToUserResponse(result.User));

    private static UserResponse ToUserResponse(UserDto user)
        => new(user.Id, user.Email, user.FirstName, user.LastName, user.Role.ToString());
}
