using InvoiceKeeper.Infrastructure.Configuration;

namespace InvoiceKeeper.Tests.Support;

/// <summary>
/// Загружает корневой <c>.env</c> (как это делает приложение), чтобы тесты
/// получали POSTGRES_* без ручного выставления переменных в IDE.
/// Уже заданные переменные окружения имеют приоритет.
/// </summary>
internal static class TestEnvironment
{
    private static readonly Lock Gate = new();
    private static bool _loaded;

    public static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        lock (Gate)
        {
            if (_loaded)
            {
                return;
            }

            // Тесты запускаются из backend/tests/bin/<Configuration>/net10.0,
            // поэтому .env ищется вверх по дереву каталогов.
            DotEnvLoader.LoadFromDirectoryTree(AppContext.BaseDirectory);
            _loaded = true;
        }
    }

    public static string? Get(string name)
    {
        EnsureLoaded();
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public static string GetRequired(string name) =>
        Get(name) ?? throw new InvalidOperationException(
            $"{name} не задан: добавьте его в .env в корне репозитория.");
}

