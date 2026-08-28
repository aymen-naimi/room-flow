using FluentValidation;

namespace RoomFlow.Application.Features.Bookings.Queries.GetBookingById;

public sealed class GetBookingByIdQueryValidator : AbstractValidator<GetBookingByIdQuery>
{
    public GetBookingByIdQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
