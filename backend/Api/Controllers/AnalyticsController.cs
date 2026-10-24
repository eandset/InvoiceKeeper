using InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceKeeper.Api.Controllers;

[ApiController]
[Route("api/v1/payments")]
[Authorize(Policy = "UserOrAdmin")]
public sealed class PaymentsSummaryController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentsSummaryController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>Итоговые суммы неоплаченных платежей по валютам.</summary>
    [HttpGet("summary")]
    [ProducesResponseType<PaymentSummaryDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaymentSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _paymentService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }
}

