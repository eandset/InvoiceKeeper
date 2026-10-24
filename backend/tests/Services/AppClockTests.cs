using InvoiceKeeper.Application.Common;
using Microsoft.Extensions.Configuration;

namespace InvoiceKeeper.Tests.Services;

public class AppClockTests
{
    [Fact]
    public void Today_UsesConfiguredTimeZone()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["APP_TIMEZONE"] = "Europe/Minsk"
            })
            .Build();
        var clock = new AppClock(configuration);

        var expected = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, clock.TimeZone).Date;

        Assert.Equal(expected, clock.Today);
    }

    [Fact]
    public void ResolveTimeZone_FallsBackToUtc_ForUnknownZone()
    {
        Assert.Equal(TimeZoneInfo.Utc, AppClock.ResolveTimeZone("Not/AZone"));
    }

    [Fact]
    public void ResolveTimeZone_FallsBackToUtc_ForEmptyValue()
    {
        Assert.Equal(TimeZoneInfo.Utc, AppClock.ResolveTimeZone(null));
        Assert.Equal(TimeZoneInfo.Utc, AppClock.ResolveTimeZone("   "));
    }

    [Fact]
    public void Today_DoesNotChangeDate_WhenTimeZoneIsUtc()
    {
        var clock = new AppClock();

        Assert.Equal(TimeZoneInfo.Utc, clock.TimeZone);
        Assert.Equal(DateTime.UtcNow.Date, clock.Today);
    }
}
