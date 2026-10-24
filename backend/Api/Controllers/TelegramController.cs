using InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceKeeper.Api.Controllers;

[ApiController]
[Route("api/v1/telegram")]
public sealed class TelegramController : ControllerBase
{
    private readonly TelegramAuthService _telegramAuthService;
    private readonly IConfiguration _configuration;

    public TelegramController(TelegramAuthService telegramAuthService, IConfiguration configuration)
    {
        _telegramAuthService = telegramAuthService;
        _configuration = configuration;
    }

    /// <summary>Привязка Telegram ID к текущей учётной записи через Telegram Login Widget.</summary>
    [HttpPost("link")]
    [Authorize(Policy = "UserOrAdmin")]
    [ProducesResponseType<TelegramProfileResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TelegramProfileResponseDto>> Link(
        [FromBody] TelegramLoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var botToken = _configuration["BOT_SERVICE_TOKEN"]
            ?? _configuration.GetSection("BotAuth")["Token"]
            ?? string.Empty;

        var result = await _telegramAuthService.LinkTelegramAsync(
            User.GetUserId(),
            request,
            botToken,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>Поиск пользователя по Telegram ID (для бота).</summary>
    [HttpGet("users/{telegramId:long}")]
    [Authorize(Policy = "ServiceOrAdmin")]
    [ProducesResponseType<TelegramProfileResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<TelegramProfileResponseDto?>> GetUserByTelegramId(
        long telegramId,
        CancellationToken cancellationToken)
    {
        var user = await _telegramAuthService.FindByTelegramIdAsync(telegramId, cancellationToken);

        if (user is null)
        {
            return NoContent();
        }

        return Ok(user);
    }
}
