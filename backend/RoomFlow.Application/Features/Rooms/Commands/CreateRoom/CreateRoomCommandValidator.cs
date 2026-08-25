using FluentValidation;

namespace RoomFlow.Application.Features.Rooms.Commands.CreateRoom;

public sealed class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Capacity)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000);

        RuleFor(command => command.Location)
            .NotEmpty()
            .MaximumLength(200);
    }
}
