using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceKeeper.Api.Controllers;

/// <summary>Редирект на Swagger UI.</summary>
[ApiController]
[Route("api/v1/docs")]
[AllowAnonymous]
public sealed class DocsController : ControllerBase
{
    [HttpGet]
    public IActionResult Index() =>
        Redirect("~/swagger/index.html");
}
