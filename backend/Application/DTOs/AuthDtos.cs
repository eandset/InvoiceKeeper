using System.ComponentModel.DataAnnotations;

namespace InvoiceKeeper.Application.DTOs;

public record LoginRequestDto(
    [Required, MaxLength(50)] string Username,
    [Required, MaxLength(128)] string Password);

public record AuthResponseDto(string Token, string Username, string Role);

public record CreateUserDto(
    [Required, MaxLength(50)] string Username,
    [Required, MinLength(8), MaxLength(128)] string Password,
    [Required] string Role);

public record UserResponseDto(Guid Id, string Username, string Role, DateTime CreatedAt);

public record TelegramLoginRequestDto(
    [Required] long Id,
    [Required] string FirstName,
    string? LastName,
    string? Username,
    [Required] string Hash,
    [Required] long AuthDate);

public record TelegramProfileResponseDto(
    Guid Id,
    string Username,
    string Role,
    long? TelegramId,
    long? TelegramChatId);
