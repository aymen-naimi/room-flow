using FluentValidation;

namespace RoomFlow.Application.Features.Bookings.Commands.DeleteBooking;

public sealed class DeleteBookingCommandValidator : AbstractValidator<DeleteBookingCommand>
{
    public DeleteBookingCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
    }
}
