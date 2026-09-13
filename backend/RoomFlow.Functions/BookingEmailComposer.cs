using System.Globalization;
using System.Net;
using RoomFlow.Application.Messaging;

namespace RoomFlow.Functions;

public static class BookingEmailComposer
{
    private static readonly TimeZoneInfo ParisZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris");
    private static readonly CultureInfo French = CultureInfo.GetCultureInfo("fr-FR");

    public static string Subject(BookingEmailMessage message)
        => message.EventType == BookingEmailEventType.Created
            ? $"Réservation confirmée — {message.RoomName}"
            : $"Réservation annulée — {message.RoomName}";

    public static string Html(BookingEmailMessage message)
    {
        var when = DescribeSlot(message);
        var intro = message.EventType == BookingEmailEventType.Created
            ? "Votre réservation a été confirmée."
            : "Cette réservation a été annulée.";

        return $"""
            <p>Bonjour {WebUtility.HtmlEncode(message.UserDisplayName)},</p>
            <p>{intro}</p>
            <ul>
              <li>Salle : {WebUtility.HtmlEncode(message.RoomName)}</li>
              <li>Date : {WebUtility.HtmlEncode(when.Date)}</li>
              <li>Heure : {WebUtility.HtmlEncode(when.Hours)}</li>
              <li>Durée : {WebUtility.HtmlEncode(when.Duration)}</li>
            </ul>
            """;
    }

    public static string PlainText(BookingEmailMessage message)
    {
        var when = DescribeSlot(message);
        var intro = message.EventType == BookingEmailEventType.Created
            ? "Votre réservation a été confirmée."
            : "Cette réservation a été annulée.";

        return $"""
            Bonjour {message.UserDisplayName},

            {intro}

            Salle : {message.RoomName}
            Date : {when.Date}
            Heure : {when.Hours}
            Durée : {when.Duration}
            """;
    }

    private static (string Date, string Hours, string Duration) DescribeSlot(BookingEmailMessage message)
    {
        var start = TimeZoneInfo.ConvertTime(message.StartsAt, ParisZone);
        var end = TimeZoneInfo.ConvertTime(message.EndsAt, ParisZone);
        var duration = end - start;
        var date = start.ToString("dddd d MMMM yyyy", French);
        var hours = $"{start:HH\\:mm} – {end:HH\\:mm}";
        var durationLabel = duration.TotalHours >= 1 && duration.Minutes == 0
            ? $"{(int)duration.TotalHours} h"
            : $"{(int)duration.TotalHours} h {duration.Minutes:00}";
        if (duration.TotalHours < 1)
        {
            durationLabel = $"{duration.Minutes} min";
        }

        return (date, hours, durationLabel);
    }
}
