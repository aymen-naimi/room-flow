using MediatR;
using Microsoft.Extensions.Logging;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Exceptions;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Features.Rooms.Commands.CreateRoom;

public record CreateRoomCommand(string Name, int Capacity, string Location, Guid CreatedByUserId)
    : IRequest<RoomDto>;

public sealed class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, RoomDto>
{
    private readonly IRoomWriteStore _writeStore;
    private readonly ILogger<CreateRoomCommandHandler> _logger;

    public CreateRoomCommandHandler(IRoomWriteStore writeStore, ILogger<CreateRoomCommandHandler> logger)
    {
        _writeStore = writeStore;
        _logger = logger;
    }

    public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        if (await _writeStore.ExistsWithNameAsync(request.Name, cancellationToken))
        {
            _logger.LogWarning(
                "Room creation rejected because name {RoomName} is already taken",
                request.Name);
            throw new RoomNameAlreadyTakenException(request.Name);
        }

        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Capacity = request.Capacity,
            Location = request.Location,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = request.CreatedByUserId
        };

        await _writeStore.AddAsync(room, cancellationToken);

        _logger.LogInformation(
            "Room created {RoomId} with name {RoomName}",
            room.Id,
            room.Name);

        return new RoomDto(room.Id, room.Name, room.Capacity, room.Location, room.CreatedAt);
    }
}
