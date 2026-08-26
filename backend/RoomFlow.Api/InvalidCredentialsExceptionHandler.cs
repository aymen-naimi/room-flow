using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RoomFlow.Application.Features.Auth.Commands.Login;

namespace RoomFlow.Api;

internal sealed class InvalidCredentialsExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not InvalidCredentialsException invalidCredentials)
        {
            return false;
        }

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = invalidCredentials.Message
        };

        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
