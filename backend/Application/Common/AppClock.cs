using Microsoft.Extensions.Configuration;

namespace InvoiceKeeper.Application.Common;

/// <summary>Часы приложения: UTC для audit-полей, дата — в часовом поясе APP_TIMEZONE.</summary>
public sealed class AppClock : IAppClock
{
    public const string DefaultTimeZoneId = "UTC";

    public AppClock(IConfiguration? configuration = null)
    {
        var timeZoneId = configuration?["APP_TIMEZONE"];
        TimeZone = ResolveTimeZone(timeZoneId);
    }

    public TimeZoneInfo TimeZone { get; }

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime Today => TimeZoneInfo.ConvertTimeFromUtc(UtcNow, TimeZone).Date;

    /// <summary>Разрешает идентификатор часового пояса, откатываясь к UTC при ошибке.</summary>
    public static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId.Trim());
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
