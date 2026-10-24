using InvoiceKeeper.Application.Common;
using InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Domain.Entities;
using InvoiceKeeper.Domain.Enums;
using InvoiceKeeper.Infrastructure.Data;
using InvoiceKeeper.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace InvoiceKeeper.Application.Services;

public sealed class AuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly JwtTokenService _tokenService;

    public AuthService(AppDbContext dbContext, IPasswordHasher passwordHasher, JwtTokenService tokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Username.ToLower() == username, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Неверное имя пользователя или пароль.");
        }

        return new AuthResponseDto(_tokenService.CreateToken(user), user.Username, user.Role.ToString());
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        if (username.Length == 0)
        {
            throw new BadRequestException("Имя пользователя не может быть пустым.");
        }

        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role) ||
            !Enum.IsDefined(role))
        {
            throw new BadRequestException("Роль должна быть 'Admin' или 'User'.");
        }

        if (role != UserRole.User)
        {
            throw new ForbiddenException("Создавать администраторов запрещено.");
        }

        var normalizedUsername = username.ToLowerInvariant();
        var exists = await _dbContext.Users
            .AnyAsync(u => u.Username.ToLower() == normalizedUsername, cancellationToken);

        if (exists)
        {
            throw new ConflictException($"Пользователь '{username}' уже существует.");
        }

        var user = new User
        {
            Username = username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = role
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UserResponseDto(user.Id, user.Username, user.Role.ToString(), user.CreatedAt);
    }

    public async Task<IReadOnlyList<UserResponseDto>> GetAllUsersAsync(CancellationToken cancellationToken) =>
        await _dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .Select(u => new UserResponseDto(u.Id, u.Username, u.Role.ToString(), u.CreatedAt))
            .ToListAsync(cancellationToken);

    public async Task<UserResponseDto> UpdateUserAsync(
        Guid id,
        UpdateUserDto request,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role) ||
            !Enum.IsDefined(role))
        {
            throw new BadRequestException("Роль должна быть 'Admin' или 'User'.");
        }

        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Пользователь {id} не найден.");

        if (user.Role == UserRole.Admin && user.Id != currentUserId)
        {
            throw new ForbiddenException("Изменение другого администратора запрещено.");
        }

        if (role != user.Role)
        {
            throw new ForbiddenException("Изменять роль пользователя запрещено.");
        }

        if (!string.IsNullOrWhiteSpace(request.Password) && request.Password != "(unchanged)")
        {
            user.PasswordHash = _passwordHasher.Hash(request.Password);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UserResponseDto(user.Id, user.Username, user.Role.ToString(), user.CreatedAt);
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Пользователь {id} не найден.");

        if (user.Role == UserRole.Admin)
        {
            throw new ForbiddenException("Удаление администратора запрещено.");
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserResponseDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserResponseDto(u.Id, u.Username, u.Role.ToString(), u.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException($"Пользователь {id} не найден.");
}

