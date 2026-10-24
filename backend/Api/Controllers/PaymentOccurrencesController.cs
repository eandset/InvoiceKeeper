using InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceKeeper.Api.Controllers;

[Route("api/v1/payment-occurrences")]
[ApiController]
public sealed class PaymentOccurrencesController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentOccurrencesController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>Обновление статуса оплаты периода (пользователь или администратор).</summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "UserOrAdmin")]
    [ProducesResponseType<PaymentOccurrenceResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentOccurrenceResponseDto>> Update(
        Guid id,
        [FromBody] UpdatePaymentStatusDto request,
        CancellationToken cancellationToken)
    {
        var occurrence = await _paymentService.UpdateStatusAsync(id, request, cancellationToken);
        return Ok(occurrence);
    }

    /// <summary>Периоды, требующие напоминания (Telegram-бот).</summary>
    [HttpGet("reminders")]
    [Authorize(Policy = "ServiceOrAdmin")]
    [ProducesResponseType<IReadOnlyList<ReminderOccurrenceDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReminderOccurrenceDto>>> GetReminders(
        [FromQuery] int withinDays = 3,
        CancellationToken cancellationToken = default)
    {
        var reminders = await _paymentService.GetRemindersAsync(withinDays, cancellationToken);
        return Ok(reminders);
    }

    /// <summary>Отметка о том, что напоминание отправлено (Telegram-бот).</summary>
    [HttpPatch("{id:guid}/reminder-sent")]
    [Authorize(Policy = "ServiceOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkReminderSent(Guid id, CancellationToken cancellationToken)
    {
        await _paymentService.MarkReminderSentAsync(id, cancellationToken);
        return NoContent();
    }
}
