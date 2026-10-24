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

    /// <summary>Пользователь по идентификатору.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<UserResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _authService.GetUserByIdAsync(id, cancellationToken);
        return Ok(user);
    }

    /// <summary>Изменение пользователя (роль и/или пароль).</summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType<UserResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> Update(
        Guid id,
        [FromBody] UpdateUserDto request,
        CancellationToken cancellationToken)
    {
        var user = await _authService.UpdateUserAsync(id, request, User.GetUserId(), cancellationToken);
        return Ok(user);
    }

    /// <summary>Удаление пользователя.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _authService.DeleteUserAsync(id, cancellationToken);
        return NoContent();
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
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }
}

