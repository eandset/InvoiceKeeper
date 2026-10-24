using InvoiceKeeper.Application.Common;
using InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Application.Services;
using InvoiceKeeper.Domain.Entities;
using InvoiceKeeper.Domain.Enums;
using InvoiceKeeper.Infrastructure.Configuration;
using InvoiceKeeper.Infrastructure.Security;
using InvoiceKeeper.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InvoiceKeeper.Tests.Services;

public class AuthServiceTests
{
    private static AuthService CreateService(Infrastructure.Data.AppDbContext dbContext) =>
        new(
            dbContext,
            new Pbkdf2PasswordHasher(),
            new JwtTokenService(Options.Create(new JwtOptions
            {
                Secret = "auth-service-tests-secret-0123456789abcdef",
                Issuer = "InvoiceKeeperBackend",
                Audience = "InvoiceKeeperClients",
                ExpiryInHours = 1
            })));

    [PostgresFact]
    public async Task CreateUserAsync_CreatesUserWithHashedPassword()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var service = CreateService(db.Context);

        var created = await service.CreateUserAsync(
            new CreateUserDto("ivan", "Strong-Passw0rd!", "User"),
            CancellationToken.None);

        Assert.Equal("ivan", created.Username);
        Assert.Equal("User", created.Role);

        var stored = await db.Context.Users.SingleAsync();
        Assert.NotEqual("Strong-Passw0rd!", stored.PasswordHash);
        Assert.True(new Pbkdf2PasswordHasher().Verify("Strong-Passw0rd!", stored.PasswordHash));
    }

    [PostgresFact]
    public async Task CreateUserAsync_ForbidsCreatingAdmin()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var service = CreateService(db.Context);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.CreateUserAsync(
                new CreateUserDto("root", "Strong-Passw0rd!", "Admin"),
                CancellationToken.None));

        Assert.Equal(0, await db.Context.Users.CountAsync());
    }

    [PostgresFact]
    public async Task CreateUserAsync_RejectsInvalidRole()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var service = CreateService(db.Context);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.CreateUserAsync(
                new CreateUserDto("ivan", "Strong-Passw0rd!", "Superuser"),
                CancellationToken.None));
    }

    [PostgresFact]
    public async Task CreateUserAsync_RejectsDuplicateUsernameIgnoringCase()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var service = CreateService(db.Context);

        await service.CreateUserAsync(
            new CreateUserDto("ivan", "Strong-Passw0rd!", "User"),
            CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateUserAsync(
                new CreateUserDto("IVAN", "Other-Passw0rd!", "User"),
                CancellationToken.None));
    }

    [PostgresFact]
    public async Task UpdateUserAsync_ForbidsChangingAnyAdmin()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var admin = new User
        {
            Username = "admin",
            PasswordHash = new Pbkdf2PasswordHasher().Hash("Admin-Passw0rd!"),
            Role = UserRole.Admin
        };
        db.Context.Users.Add(admin);
        await db.Context.SaveChangesAsync();

        var service = CreateService(db.Context);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.UpdateUserAsync(
                admin.Id,
                new UpdateUserDto("Changed-Passw0rd!", "Admin"),
                admin.Id,
                CancellationToken.None));
    }

    [PostgresFact]
    public async Task UpdateUserAsync_ChangesPasswordForRegularUser()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var user = new User
        {
            Username = "ivan",
            PasswordHash = new Pbkdf2PasswordHasher().Hash("Initial-Passw0rd!"),
            Role = UserRole.User
        };
        db.Context.Users.Add(user);
        await db.Context.SaveChangesAsync();

        var service = CreateService(db.Context);

        await service.UpdateUserAsync(
            user.Id,
            new UpdateUserDto("Changed-Passw0rd!", "User"),
            Guid.NewGuid(),
            CancellationToken.None);

        var stored = await db.Context.Users.SingleAsync();
        Assert.True(new Pbkdf2PasswordHasher().Verify("Changed-Passw0rd!", stored.PasswordHash));
    }

    [PostgresFact]
    public async Task UpdateUserAsync_ForbidsChangingRole()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var user = new User
        {
            Username = "ivan",
            PasswordHash = new Pbkdf2PasswordHasher().Hash("Initial-Passw0rd!"),
            Role = UserRole.User
        };
        db.Context.Users.Add(user);
        await db.Context.SaveChangesAsync();

        var service = CreateService(db.Context);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.UpdateUserAsync(
                user.Id,
                new UpdateUserDto("(unchanged)", "Admin"),
                Guid.NewGuid(),
                CancellationToken.None));
    }

    [PostgresFact]
    public async Task DeleteUserAsync_ForbidsDeletingAdmin()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var admin = new User
        {
            Username = "admin",
            PasswordHash = new Pbkdf2PasswordHasher().Hash("Admin-Passw0rd!"),
            Role = UserRole.Admin
        };
        db.Context.Users.Add(admin);
        await db.Context.SaveChangesAsync();

        var service = CreateService(db.Context);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.DeleteUserAsync(admin.Id, CancellationToken.None));
    }

    [PostgresFact]
    public async Task DeleteUserAsync_RemovesRegularUser()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        var user = new User
        {
            Username = "ivan",
            PasswordHash = new Pbkdf2PasswordHasher().Hash("Initial-Passw0rd!"),
            Role = UserRole.User
        };
        db.Context.Users.Add(user);
        await db.Context.SaveChangesAsync();

        var service = CreateService(db.Context);
        await service.DeleteUserAsync(user.Id, CancellationToken.None);

        Assert.Equal(0, await db.Context.Users.CountAsync());
    }

    [PostgresFact]
    public async Task LoginAsync_ReturnsToken_ForValidCredentials()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        db.Context.Users.Add(new User
        {
            Username = "ivan",
            PasswordHash = new Pbkdf2PasswordHasher().Hash("Initial-Passw0rd!"),
            Role = UserRole.User
        });
        await db.Context.SaveChangesAsync();

        var service = CreateService(db.Context);
        var response = await service.LoginAsync(
            new LoginRequestDto("Ivan", "Initial-Passw0rd!"),
            CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal("ivan", response.Username);
        Assert.Equal("User", response.Role);
    }

    [PostgresFact]
    public async Task LoginAsync_Throws_ForWrongPassword()
    {
        await using var db = await PostgresTestDatabase.CreateAsync();
        db.Context.Users.Add(new User
        {
            Username = "ivan",
            PasswordHash = new Pbkdf2PasswordHasher().Hash("Initial-Passw0rd!"),
            Role = UserRole.User
        });
        await db.Context.SaveChangesAsync();

        var service = CreateService(db.Context);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.LoginAsync(new LoginRequestDto("ivan", "wrong"), CancellationToken.None));
    }
}
