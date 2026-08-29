using MediatR;
using RoomFlow.Application.Abstractions.Data;
using RoomFlow.Application.Exceptions;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Application.Features.Rooms.Commands.CreateRoom;

public record CreateRoomCommand(string Name, int Capacity, string Location) : IRequest<RoomDto>;

public sealed class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, RoomDto>
{
    private readonly IRoomWriteStore _writeStore;

    public CreateRoomCommandHandler(IRoomWriteStore writeStore)
    {
        _writeStore = writeStore;
    }

    public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        if (await _writeStore.ExistsWithNameAsync(request.Name, cancellationToken))
        {
            throw new RoomNameAlreadyTakenException(request.Name);
        }

        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Capacity = request.Capacity,
            Location = request.Location,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _writeStore.AddAsync(room, cancellationToken);

        return new RoomDto(room.Id, room.Name, room.Capacity, room.Location, room.CreatedAt);
    }
}
