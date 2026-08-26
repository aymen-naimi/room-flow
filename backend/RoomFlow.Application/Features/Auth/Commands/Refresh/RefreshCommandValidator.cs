using FluentValidation;

namespace RoomFlow.Application.Features.Auth.Commands.Refresh;

public sealed class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty()
            .MaximumLength(200);
    }
}
