using Npgsql;

namespace InvoiceKeeper.Tests.Support;

/// <summary>Создание и удаление временной базы PostgreSQL для интеграционных тестов.</summary>
internal static class PostgresTestServer
{
    public static async Task<(string DatabaseName, string ConnectionString)> CreateDatabaseAsync()
    {
        var password = TestEnvironment.GetRequired("POSTGRES_PASSWORD");
        var host = TestEnvironment.Get("POSTGRES_HOST") ?? "localhost";
        var port = TestEnvironment.Get("POSTGRES_PORT") ?? "5432";
        var username = TestEnvironment.Get("POSTGRES_USER") ?? "postgres";
        var databaseName = $"invoice_keeper_test_{Guid.NewGuid():N}";

        var adminBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = int.Parse(port),
            Database = "postgres",
            Username = username,
            Password = password
        };

        await using (var admin = new NpgsqlConnection(adminBuilder.ConnectionString))
        {
            await admin.OpenAsync();
            await using var create = admin.CreateCommand();
            create.CommandText = $"CREATE DATABASE \"{databaseName}\"";
            await create.ExecuteNonQueryAsync();
        }

        adminBuilder.Database = databaseName;
        return (databaseName, adminBuilder.ConnectionString);
    }

    public static async Task DropDatabaseAsync(string databaseName)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = TestEnvironment.Get("POSTGRES_HOST") ?? "localhost",
            Port = int.Parse(TestEnvironment.Get("POSTGRES_PORT") ?? "5432"),
            Database = "postgres",
            Username = TestEnvironment.Get("POSTGRES_USER") ?? "postgres",
            Password = TestEnvironment.GetRequired("POSTGRES_PASSWORD")
        };

        await using var admin = new NpgsqlConnection(builder.ConnectionString);
        await admin.OpenAsync();
        await using var drop = admin.CreateCommand();
        drop.CommandText = $"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)";
        await drop.ExecuteNonQueryAsync();
    }
}

