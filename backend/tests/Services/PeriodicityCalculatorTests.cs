using InvoiceKeeper.Application.Services;

namespace InvoiceKeeper.Tests.Services;

public class PeriodicityCalculatorTests
{
    [Theory]
    [InlineData("monthly", "2026-01-31", "2026-02-28")]
    [InlineData("weekly", "2026-01-31", "2026-02-07")]
    [InlineData("quarterly", "2026-01-31", "2026-04-30")]
    [InlineData("yearly", "2024-02-29", "2025-02-28")]
    public void Next_CalculatesExpectedDate(string periodicity, string dueDate, string expected)
    {
        var result = PeriodicityCalculator.Next(DateTime.Parse(dueDate), periodicity);

        Assert.Equal(DateTime.Parse(expected).Date, result);
    }

    [Theory]
    [InlineData("2024-01-31", 1, "2024-02-29")]
    [InlineData("2023-01-31", 1, "2023-02-28")]
    [InlineData("2023-01-31", 2, "2023-03-31")]
    [InlineData("2023-01-31", 3, "2023-04-30")]
    [InlineData("2023-08-31", 1, "2023-09-30")]
    [InlineData("2024-02-29", 12, "2025-02-28")]
    public void AddMonths_HandlesEndOfMonthCorrectly(string date, int months, string expected)
    {
        var result = DateTimeHelper.AddMonths(DateTime.Parse(date), months);

        Assert.Equal(DateTime.Parse(expected).Date, result);
    }

    [Theory]
    [InlineData("2024-02-29", 1, "2025-02-28")]
    [InlineData("2023-02-28", 1, "2024-02-29")]
    [InlineData("2020-02-29", 4, "2024-02-29")]
    public void AddYears_HandlesLeapYearCorrectly(string date, int years, string expected)
    {
        var result = DateTimeHelper.AddYears(DateTime.Parse(date), years);

        Assert.Equal(DateTime.Parse(expected).Date, result);
    }

    [Theory]
    [InlineData(null, "monthly")]
    [InlineData("", "monthly")]
    [InlineData("MONTHLY", "monthly")]
    [InlineData(" Weekly ", "weekly")]
    public void Normalize_ReturnsCanonicalValue(string? input, string expected)
    {
        Assert.Equal(expected, PeriodicityCalculator.Normalize(input));
    }

    [Theory]
    [InlineData("daily")]
    [InlineData("2 weeks")]
    public void Normalize_ReturnsNullForUnsupportedPeriodicity(string input)
    {
        Assert.Null(PeriodicityCalculator.Normalize(input));
    }
}
