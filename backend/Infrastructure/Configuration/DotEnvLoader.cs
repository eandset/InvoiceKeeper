namespace InvoiceKeeper.Infrastructure.Configuration;

/// <summary>
/// Минимальный загрузчик .env без внешних зависимостей.
/// Переменные, уже заданные в окружении (docker compose, CI), имеют приоритет.
/// </summary>
public static class DotEnvLoader
{
    public static void LoadFromDirectoryTree(string startDirectory)
    {
        for (var directory = new DirectoryInfo(startDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, ".env");
            if (File.Exists(candidate))
            {
                Load(candidate);
                return;
            }
        }
    }

    public static void Load(string path)
    {
        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim().Trim('"', '\'');

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
