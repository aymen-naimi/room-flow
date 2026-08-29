using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RoomFlow.Application.Features.Auth.Commands.Login;
using RoomFlow.Application.Features.Bookings.Commands.CreateBooking;
using RoomFlow.Application.Features.Bookings.Commands.DeleteBooking;
using RoomFlow.Application.Features.Rooms.Commands.CreateRoom;
using RoomFlow.Application.Features.Users.Commands.CreateUser;

namespace RoomFlow.Api;

internal sealed class ApplicationProblemExceptionHandler : IExceptionHandler
{
    private static readonly Dictionary<Type, (int Status, string Title)> Map = new()
    {
        [typeof(RoomNameAlreadyTakenException)] = (StatusCodes.Status409Conflict, "Conflict"),
        [typeof(EmailAlreadyTakenException)] = (StatusCodes.Status409Conflict, "Conflict"),
        [typeof(BookingOverlapException)] = (StatusCodes.Status409Conflict, "Conflict"),
        [typeof(InvalidCredentialsException)] = (StatusCodes.Status401Unauthorized, "Unauthorized"),
        [typeof(BookingNotOwnedException)] = (StatusCodes.Status403Forbidden, "Forbidden"),
        [typeof(RoomNotFoundException)] = (StatusCodes.Status404NotFound, "Not Found"),
    };

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (!Map.TryGetValue(exception.GetType(), out var mapping))
        {
            return false;
        }

        var problem = new ProblemDetails
        {
            Status = mapping.Status,
            Title = mapping.Title,
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = mapping.Status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
