using MediatR;
using RoomFlow.Application.Abstractions.Data;

namespace RoomFlow.Application.Features.Rooms.Commands.DeleteRoom;

public record DeleteRoomCommand(Guid Id) : IRequest<bool>;

public sealed class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand, bool>
{
    private readonly IRoomWriteStore _writeStore;

    public DeleteRoomCommandHandler(IRoomWriteStore writeStore)
    {
        _writeStore = writeStore;
    }

    public Task<bool> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
        => _writeStore.RemoveAsync(request.Id, cancellationToken);
}
