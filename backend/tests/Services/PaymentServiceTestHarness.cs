using InvoiceKeeper.Infrastructure.Data;
using InvoiceKeeper.Tests.Support;
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
        var (databaseName, databaseConnectionString) = await PostgresTestServer.CreateDatabaseAsync();

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

        await PostgresTestServer.DropDatabaseAsync(_databaseName);
    }
}

