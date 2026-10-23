using InvoiceKeeper.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvoiceKeeper.Tests.Services;

/// <summary>
/// PostgreSQL-база для интеграционных тестов: подключается к локальному серверу,
/// создаёт временную БД и удаляет её после теста. Пропускается, если сервер недоступен.
/// </summary>
public sealed class PostgresTestDatabase : IAsyncDisposable
{
    private readonly string _databaseName;

    private PostgresTestDatabase(string databaseName, AppDbContext context)
    {
        _databaseName = databaseName;
        Context = context;
    }

    public AppDbContext Context { get; }

    public static async Task<PostgresTestDatabase> CreateAsync()
    {
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
        var databaseName = $"invoice_keeper_test_{Guid.NewGuid():N}";

        var adminConnectionString =
            $"Host={host};Port={port};Database=postgres;Username={username};Password={password}";
        var databaseConnectionString =
            $"Host={host};Port={port};Database={databaseName};Username={username};Password={password}";

        await using (var admin = new Npgsql.NpgsqlConnection(adminConnectionString))
        {
            await admin.OpenAsync();
            await using var create = admin.CreateCommand();
            create.CommandText = $"CREATE DATABASE \"{databaseName}\"";
            await create.ExecuteNonQueryAsync();
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(databaseConnectionString)
            .Options;
        var context = new AppDbContext(options);
        await context.Database.MigrateAsync();

        return new PostgresTestDatabase(databaseName, context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.Database.CloseConnectionAsync();
        await Context.DisposeAsync();

        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")!;
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";

        await using var admin = new Npgsql.NpgsqlConnection(
            $"Host={host};Port={port};Database=postgres;Username={username};Password={password}");
        await admin.OpenAsync();
        await using var drop = admin.CreateCommand();
        drop.CommandText = $"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)";
        await drop.ExecuteNonQueryAsync();
    }
}

