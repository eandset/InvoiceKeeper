using InvoiceKeeper.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvoiceKeeper.Infrastructure.Data;

/// <summary>
/// Фабрика для `dotnet ef`: использует те же настройки, что и приложение, включая .env.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var projectDirectory = FindProjectDirectory();
        DotEnvLoader.LoadFromDirectoryTree(projectDirectory);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(projectDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(DatabaseConnectionString.Resolve(configuration));

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string FindProjectDirectory()
    {
        // `dotnet ef` может запускаться из корня репозитория или из каталога проекта.
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "appsettings.json")) &&
                    File.Exists(Path.Combine(directory.FullName, "backend.csproj")))
                {
                    return directory.FullName;
                }
            }
        }

        return Directory.GetCurrentDirectory();
    }
}
