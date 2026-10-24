using InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceKeeper.Api.Controllers;

[ApiController]
[Route("api/v1/analytics")]
[Authorize(Policy = "UserOrAdmin")]
public sealed class AnalyticsController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public AnalyticsController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>Список просроченных периодов.</summary>
    [HttpGet("overdue")]
    [ProducesResponseType<IReadOnlyList<PaymentOccurrenceResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PaymentOccurrenceResponseDto>>> GetOverdue(
        CancellationToken cancellationToken)
    {
        var overdue = await _paymentService.GetOverdueAsync(cancellationToken);
        return Ok(overdue);
    }
}
