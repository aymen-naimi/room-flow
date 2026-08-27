using FluentValidation;

namespace RoomFlow.Application.Features.Rooms.Commands.DeleteRoom;

public sealed class DeleteRoomCommandValidator : AbstractValidator<DeleteRoomCommand>
{
    public DeleteRoomCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
