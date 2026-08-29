using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomFlow.Api.Contracts.Rooms;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Features.Rooms.Commands.CreateRoom;
using RoomFlow.Application.Features.Rooms.Commands.DeleteRoom;
using RoomFlow.Application.Features.Rooms.Queries.GetRoomById;
using RoomFlow.Application.Features.Rooms.Queries.GetRooms;

namespace RoomFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class RoomsController : ControllerBase
{
    private readonly ISender _sender;

    public RoomsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomResponse>>> Get(CancellationToken cancellationToken)
    {
        var rooms = await _sender.Send(new GetRoomsQuery(), cancellationToken);
        return Ok(rooms.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoomResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var room = await _sender.Send(new GetRoomByIdQuery(id), cancellationToken);
        return room is null ? NotFound() : Ok(ToResponse(room));
    }

    [HttpPost]
    public async Task<ActionResult<RoomResponse>> Create(
        [FromBody] CreateRoomRequest request,
        CancellationToken cancellationToken)
    {
        if (!UserClaims.TryGetUserId(User, out var userId))
        {
            return Unauthorized();
        }

        var command = new CreateRoomCommand(request.Name, request.Capacity, request.Location, userId);
        var room = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = room.Id }, ToResponse(room));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _sender.Send(new DeleteRoomCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private static RoomResponse ToResponse(RoomDto room)
        => new(room.Id, room.Name, room.Capacity, room.Location, room.CreatedAt);
}
