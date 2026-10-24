using InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceKeeper.Api.Controllers;

[Route("api/v1/recurring-payments")]
[Authorize(Policy = "UserOrAdmin")]
[ApiController]
public sealed class RecurringPaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public RecurringPaymentsController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>Список активных платежей с ближайшим неоплаченным периодом.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PaymentResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PaymentResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var payments = await _paymentService.GetActivePaymentsAsync(cancellationToken);
        return Ok(payments);
    }

    /// <summary>Активный платёж по идентификатору.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<PaymentResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.GetByIdAsync(id, cancellationToken);
        return Ok(payment);
    }

    /// <summary>Создание регулярного платежа. Первый период создаётся автоматически.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType<PaymentResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentResponseDto>> Create(
        [FromBody] CreatePaymentDto request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
    }

    /// <summary>Мягкое удаление платежа (только администратор).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _paymentService.SoftDeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
