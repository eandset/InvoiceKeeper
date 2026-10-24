namespace InvoiceKeeper.Application.Common;

/// <summary>
/// Единый источник текущего времени для бизнес-логики.
/// Позволяет учитывать часовой пояс приложения (APP_TIMEZONE) и подменять время в тестах.
/// </summary>
public interface IAppClock
{
    /// <summary>Текущий момент в UTC (для audit-полей).</summary>
    DateTime UtcNow { get; }

    /// <summary>Текущая дата в часовом поясе приложения.</summary>
    DateTime Today { get; }

    /// <summary>Часовой пояс приложения.</summary>
    TimeZoneInfo TimeZone { get; }
}
