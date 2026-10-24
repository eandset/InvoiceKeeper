namespace InvoiceKeeper.Application.Services;

/// <summary>Фоновый сервис, который раз в день пересчитывает просроченные периоды.</summary>
public sealed class OverdueRecalculationHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OverdueRecalculationHostedService> _logger;

    public OverdueRecalculationHostedService(
        IServiceProvider serviceProvider,
        ILogger<OverdueRecalculationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис пересчёта просрочек запущен.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<OverdueRecalculationService>();
                var updated = await service.RecalculateAsync(stoppingToken);
                _logger.LogInformation("Пересчёт просрочек завершён. Обновлено записей: {Count}.", updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при пересчёте просрочек.");
            }

            var nextRun = DateTime.UtcNow.Date.AddDays(1).AddHours(1);
            var delay = nextRun - DateTime.UtcNow;

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, stoppingToken);
            }
            else
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
