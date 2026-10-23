using InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceKeeper.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize(Policy = "AdminOnly")]
public sealed class UsersController : ControllerBase
{
    private readonly AuthService _authService;

    public UsersController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Список пользователей (только администратор).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _authService.GetAllUsersAsync(cancellationToken);
        return Ok(users);
    }

    /// <summary>Создание нового пользователя (только администратор).</summary>
    [HttpPost]
    [ProducesResponseType<UserResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponseDto>> Create(
        [FromBody] CreateUserDto request,
        CancellationToken cancellationToken)
    {
        var user = await _authService.CreateUserAsync(request, cancellationToken);
        return Created($"/api/v1/users/{user.Id}", user);
    }
}

