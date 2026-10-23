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

    /// <summary>Сводка по неоплаченным счетам в разрезе валют и количеству периодов.</summary>
    [HttpGet("summary")]
    [ProducesResponseType<PaymentSummaryDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaymentSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _paymentService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }
}

