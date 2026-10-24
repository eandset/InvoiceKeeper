using System.Security.Cryptography;
using System.Text;
using InvoiceKeeper.Application.Common;
using InvoiceKeeper.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using InvoiceKeeper.Infrastructure.Data;

namespace InvoiceKeeper.Application.Services;

/// <summary>Сервис для связки учётной записи с Telegram через Telegram Login Widget.</summary>
public sealed class TelegramAuthService
{
    private readonly AppDbContext _dbContext;

    public TelegramAuthService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Привязывает Telegram ID к текущему авторизованному пользователю.</summary>
    public async Task<TelegramProfileResponseDto> LinkTelegramAsync(
        Guid userId,
        TelegramLoginRequestDto request,
        string botToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(botToken))
        {
            throw new InvalidOperationException("Telegram bot token не задан.");
        }

        if (!VerifyTelegramLoginWidget(request, botToken))
        {
            throw new UnauthorizedException("Невозможно проверить подпись Telegram Login Widget.");
        }

        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException($"Пользователь {userId} не найден.");

        if (user.TelegramId.HasValue && user.TelegramId != request.Id)
        {
            throw new ConflictException("Учётная запись уже привязана к другому Telegram.");
        }

        var existingUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.TelegramId == request.Id && u.Id != userId, cancellationToken);

        if (existingUser is not null)
        {
            throw new ConflictException("Этот Telegram уже привязан к другой учётной записи.");
        }

        user.TelegramId = request.Id;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TelegramProfileResponseDto(
            user.Id,
            user.Username,
            user.Role.ToString(),
            user.TelegramId,
            user.TelegramChatId);
    }

    /// <summary>Ищет пользователя по Telegram ID.</summary>
    public async Task<TelegramProfileResponseDto?> FindByTelegramIdAsync(
        long telegramId,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new TelegramProfileResponseDto(
            user.Id,
            user.Username,
            user.Role.ToString(),
            user.TelegramId,
            user.TelegramChatId);
    }

    /// <summary>Обновляет chat_id для существующего пользователя с telegram_id.</summary>
    public async Task<TelegramProfileResponseDto?> UpdateChatIdAsync(
        long telegramId,
        long chatId,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        user.TelegramChatId = chatId;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TelegramProfileResponseDto(
            user.Id,
            user.Username,
            user.Role.ToString(),
            user.TelegramId,
            user.TelegramChatId);
    }

    /// <summary>
    /// Проверяет подлинность данных от Telegram Login Widget.
    /// Алгоритм: https://core.telegram.org/widgets/login
    /// </summary>
    private static bool VerifyTelegramLoginWidget(TelegramLoginRequestDto request, string botToken)
    {
        var dataCheckString = BuildDataCheckString(request);
        var secretKey = ComputeSha256(botToken);
        using var hmac = new HMACSHA256(secretKey);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString));
        var hashHex = Convert.ToHexString(hash).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hashHex),
            Encoding.UTF8.GetBytes(request.Hash.ToLowerInvariant()));
    }

    private static string BuildDataCheckString(TelegramLoginRequestDto request)
    {
        var lines = new List<string>();

        lines.Add($"auth_date={request.AuthDate}");
        lines.Add($"first_name={request.FirstName}");
        lines.Add($"id={request.Id}");

        if (!string.IsNullOrEmpty(request.LastName))
        {
            lines.Add($"last_name={request.LastName}");
        }

        if (!string.IsNullOrEmpty(request.Username))
        {
            lines.Add($"username={request.Username}");
        }

        return string.Join("\n", lines);
    }

    private static byte[] ComputeSha256(string input)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(input));
    }
}
