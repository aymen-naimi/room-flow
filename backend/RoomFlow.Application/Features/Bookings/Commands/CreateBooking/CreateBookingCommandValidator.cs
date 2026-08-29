using FluentValidation;

namespace RoomFlow.Application.Features.Bookings.Commands.CreateBooking;

public sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(command => command.RoomId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();

        RuleFor(command => command.EndsAt)
            .GreaterThan(command => command.StartsAt);

        RuleFor(command => command)
            .Must(command => command.EndsAt - command.StartsAt >= TimeSpan.FromMinutes(15))
            .WithName(nameof(CreateBookingCommand.EndsAt))
            .WithMessage("The booking must last at least 15 minutes.")
            .Must(command => command.EndsAt - command.StartsAt <= TimeSpan.FromHours(12))
            .WithName(nameof(CreateBookingCommand.EndsAt))
            .WithMessage("The booking must last at most 12 hours.");

        RuleFor(command => command.StartsAt)
            .Must(startsAt => startsAt >= timeProvider.GetUtcNow())
            .WithMessage("StartsAt cannot be in the past.");
    }
}
