using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RoomFlow.Application.Features.Rooms.Commands.CreateRoom;

namespace RoomFlow.Api;

internal sealed class RoomNameAlreadyTakenExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not RoomNameAlreadyTakenException roomNameAlreadyTaken)
        {
            return false;
        }

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Detail = roomNameAlreadyTaken.Message
        };

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
