using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomFlow.Api.Contracts.Bookings;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Features.Bookings.Commands.CreateBooking;
using RoomFlow.Application.Features.Bookings.Commands.DeleteBooking;
using RoomFlow.Application.Features.Bookings.Queries.GetBookingById;
using RoomFlow.Application.Features.Bookings.Queries.GetBookings;

namespace RoomFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class BookingsController : ControllerBase
{
    private readonly ISender _sender;

    public BookingsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> Get(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] Guid? roomId,
        [FromQuery] bool mine,
        CancellationToken cancellationToken)
    {
        Guid? userId = null;
        if (mine)
        {
            if (!UserClaims.TryGetUserId(User, out var currentUserId))
            {
                return Unauthorized();
            }

            userId = currentUserId;
        }

        var bookings = await _sender.Send(
            new GetBookingsQuery(from, to, roomId, userId),
            cancellationToken);
        return Ok(bookings.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var booking = await _sender.Send(new GetBookingByIdQuery(id), cancellationToken);
        return booking is null ? NotFound() : Ok(ToResponse(booking));
    }

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        if (!UserClaims.TryGetUserId(User, out var userId))
        {
            return Unauthorized();
        }

        var command = new CreateBookingCommand(request.RoomId, userId, request.StartsAt, request.EndsAt);
        var booking = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, ToResponse(booking));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!UserClaims.TryGetUserId(User, out var userId))
        {
            return Unauthorized();
        }

        var deleted = await _sender.Send(new DeleteBookingCommand(id, userId), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private static BookingResponse ToResponse(BookingDto booking)
        => new(
            booking.Id,
            booking.RoomId,
            booking.RoomName,
            booking.UserId,
            booking.UserDisplayName,
            booking.StartsAt,
            booking.EndsAt);
}
