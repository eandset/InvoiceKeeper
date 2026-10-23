namespace InvoiceKeeper.Application.Services;

/// <summary>Расчёт следующей даты платежа для поддерживаемых видов периодичности.</summary>
public static class PeriodicityCalculator
{
    public const string Monthly = "monthly";
    public const string Weekly = "weekly";
    public const string Quarterly = "quarterly";
    public const string Yearly = "yearly";

    public static readonly IReadOnlyList<string> Supported = new[]
    {
        Monthly, Weekly, Quarterly, Yearly
    };

    /// <summary>Приводит периодичность к каноническому виду либо возвращает null, если значение неизвестно.</summary>
    public static string? Normalize(string? periodicity)
    {
        if (string.IsNullOrWhiteSpace(periodicity))
        {
            return Monthly;
        }

        var value = periodicity.Trim().ToLowerInvariant();
        return Supported.Contains(value) ? value : null;
    }

    public static DateTime Next(DateTime dueDate, string periodicity) =>
        Normalize(periodicity) switch
        {
            Weekly => dueDate.Date.AddDays(7),
            Quarterly => dueDate.Date.AddMonths(3),
            Yearly => dueDate.Date.AddYears(1),
            _ => dueDate.Date.AddMonths(1)
        };
}

