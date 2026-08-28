using System.Globalization;
using System.Text.RegularExpressions;
using FluentValidation;

namespace RoomFlow.Application.Features.Bookings.Queries.GetBookings;

public sealed class GetBookingsQueryValidator : AbstractValidator<GetBookingsQuery>
{
    private static readonly Regex OffsetSuffix = new(@"[+-]\d{2}:\d{2}$", RegexOptions.Compiled);

    public GetBookingsQueryValidator()
    {
        RuleFor(query => query.From)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(BeIso8601Instant)
            .WithMessage("from must be a valid ISO-8601 timestamp with an offset or Z.");

        RuleFor(query => query.To)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(BeIso8601Instant)
            .WithMessage("to must be a valid ISO-8601 timestamp with an offset or Z.");

        RuleFor(query => query)
            .Must(query => Parse(query.From!) < Parse(query.To!))
            .When(query => BeIso8601Instant(query.From) && BeIso8601Instant(query.To))
            .WithMessage("from must be before to.");

        RuleFor(query => query)
            .Must(query => Parse(query.To!) - Parse(query.From!) <= TimeSpan.FromDays(8))
            .When(query => BeIso8601Instant(query.From) && BeIso8601Instant(query.To)
                && Parse(query.From!) < Parse(query.To!))
            .WithMessage("The requested range must not exceed 8 days.");

        RuleFor(query => query.RoomId)
            .Must(roomId => roomId is null || roomId != Guid.Empty)
            .WithMessage("roomId must be a valid GUID.");
    }

    public static bool BeIso8601Instant(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
        {
            return false;
        }

        return value.EndsWith("Z", StringComparison.OrdinalIgnoreCase)
            || OffsetSuffix.IsMatch(value);
    }

    private static DateTimeOffset Parse(string value)
        => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
}
