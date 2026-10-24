using InvoiceKeeper.Domain.Entities;
using InvoiceKeeper.Domain.Enums;
using InvoiceKeeper.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace InvoiceKeeper.Infrastructure.Data.Seed;

/// <summary>Создаёт учётную запись администратора из переменных окружения при первом запуске.</summary>
public sealed class AdminSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminSeeder> _logger;

    public AdminSeeder(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<AdminSeeder> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var username = FirstNonEmpty(
            _configuration["AdminSeed:Username"],
            _configuration["ADMIN_USERNAME"]);
        var password = FirstNonEmpty(
            _configuration["AdminSeed:Password"],
            _configuration["ADMIN_PASSWORD"]);

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning(
                "Администратор не создан: задайте ADMIN_USERNAME и ADMIN_PASSWORD (или AdminSeed:* в конфигурации).");
            return;
        }

        username = username.Trim();
        var normalizedUsername = username.ToLowerInvariant();

        var exists = await _dbContext.Users
            .AnyAsync(u => u.Username.ToLower() == normalizedUsername, cancellationToken);

        if (exists)
        {
            _logger.LogInformation("Учётная запись администратора {Username} уже существует.", username);
            return;
        }

        _dbContext.Users.Add(new User
        {
            Username = username,
            PasswordHash = _passwordHasher.Hash(password),
            Role = UserRole.Admin
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Создана учётная запись администратора {Username}.", username);
    }

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}

