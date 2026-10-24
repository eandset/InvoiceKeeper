using System.ComponentModel.DataAnnotations;

namespace InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Application.Validation;

public record LoginRequestDto(
    [Required, MaxLength(50)] string Username,
    [Required, MaxLength(128)] string Password);

public record AuthResponseDto(string Token, string Username, string Role);

public record CreateUserDto(
    [Required, MaxLength(50), RegularExpression(@"^[a-zA-Z0-9_-]{3,50}$", ErrorMessage = "Имя пользователя должно содержать 3-50 символов: буквы, цифры, '_' или '-'." )] string Username,
    [Required, MinLength(8), MaxLength(128)] string Password,
    [Required, AllowedValues("Admin", "User")] string Role);

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
