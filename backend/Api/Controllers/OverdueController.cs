using InvoiceKeeper.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceKeeper.Api.Controllers;

[ApiController]
[Route("api/v1/payment-occurrences")]
[Authorize(Policy = "AdminOnly")]
public sealed class OverdueController : ControllerBase
{
    private readonly OverdueRecalculationService _overdueRecalculationService;

    public OverdueController(OverdueRecalculationService overdueRecalculationService)
    {
        _overdueRecalculationService = overdueRecalculationService;
    }

    /// <summary>Запускает ручной пересчёт просроченных периодов.</summary>
    [HttpPost("recalculate-overdue")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> RecalculateOverdue(CancellationToken cancellationToken)
    {
        var updated = await _overdueRecalculationService.RecalculateAsync(cancellationToken);
        return Ok(updated);
    }
}
