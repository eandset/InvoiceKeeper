using Microsoft.Extensions.Configuration;

namespace InvoiceKeeper.Application.Services;

/// <summary>Утилиты для работы с датами и часовыми поясами.</summary>
public static class DateTimeHelper
{
    /// <summary>Возвращает текущую дату в едином часовом поясе приложения.</summary>
    /// <remarks>По умолчанию используется UTC, если APP_TIMEZONE не задан или невалиден.</remarks>
    public static DateTime Today(IConfiguration? configuration = null)
    {
        var timeZoneId = configuration?["APP_TIMEZONE"] ?? "UTC";
        var timeZone = GetTimeZone(timeZoneId);
        var now = DateTime.UtcNow;
        var local = TimeZoneInfo.ConvertTimeFromUtc(now, timeZone);
        return local.Date;
    }

    /// <summary>Возвращает текущее время в едином часовом поясе приложения.</summary>
    public static DateTime Now(IConfiguration? configuration = null)
    {
        var timeZoneId = configuration?["APP_TIMEZONE"] ?? "UTC";
        var timeZone = GetTimeZone(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
    }

    /// <summary>Добавляет месяцы к дате, корректируя последний день месяца при необходимости.</summary>
    public static DateTime AddMonths(DateTime date, int months)
    {
        var result = date.AddMonths(months);
        return result;
    }

    /// <summary>Добавляет годы к дате, корректируя последний день месяца при необходимости.</summary>
    public static DateTime AddYears(DateTime date, int years)
    {
        var result = date.AddYears(years);
        return result;
    }

    private static TimeZoneInfo GetTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
