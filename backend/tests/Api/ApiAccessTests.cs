using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InvoiceKeeper.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace InvoiceKeeper.Tests.Api;

/// <summary>Матрица доступа к HTTP API для ролей Admin, User и Service.</summary>
[Collection(ApiCollection.Name)]
public sealed class ApiAccessTests
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ApiAccessTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ---------- Без аутентификации ----------

    [Theory]
    [InlineData("GET", "/api/v1/recurring-payments")]
    [InlineData("POST", "/api/v1/recurring-payments")]
    [InlineData("GET", "/api/v1/recurring-payments/6a1f3c1e-0000-0000-0000-000000000000")]
    [InlineData("DELETE", "/api/v1/recurring-payments/6a1f3c1e-0000-0000-0000-000000000000")]
    [InlineData("GET", "/api/v1/payments/summary")]
    [InlineData("GET", "/api/v1/users")]
    [InlineData("POST", "/api/v1/users")]
    [InlineData("PATCH", "/api/v1/users/6a1f3c1e-0000-0000-0000-000000000000")]
    [InlineData("DELETE", "/api/v1/users/6a1f3c1e-0000-0000-0000-000000000000")]
    [InlineData("GET", "/api/v1/auth/me")]
    [InlineData("POST", "/api/v1/auth/logout")]
    [InlineData("GET", "/api/v1/payment-occurrences")]
    [InlineData("GET", "/api/v1/payment-occurrences/6a1f3c1e-0000-0000-0000-000000000000")]
    [InlineData("GET", "/api/v1/payment-occurrences/reminders")]
    [InlineData("PATCH", "/api/v1/payment-occurrences/6a1f3c1e-0000-0000-0000-000000000000")]
    [InlineData("PATCH", "/api/v1/payment-occurrences/6a1f3c1e-0000-0000-0000-000000000000/reminder-sent")]
    public async Task ProtectedEndpoints_Return401_WithoutToken(string method, string url)
    {
        var response = await SendAsync(new HttpMethod(method), url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Payments_Return401_ForMalformedJwt()
    {
        var response = await SendAsync(HttpMethod.Get, "/api/v1/recurring-payments", token: "not-a-jwt");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Reminders_Return401_ForWrongServiceToken()
    {
        var response = await SendAsync(
            HttpMethod.Get,
            "/api/v1/payment-occurrences/reminders",
            serviceToken: "wrong-token");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_Returns401_ForWrongPassword()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { username = ApiFactory.UserUsername, password = "wrong-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Docs_RedirectsToSwaggerUi_WithoutToken()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/v1/docs");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/swagger/index.html", response.Headers.Location!.ToString());
    }

    // ---------- Роль User ----------

    [Fact]
    public async Task User_CanReadPaymentsAndAnalytics()
    {
        var token = await LoginAsync(ApiFactory.UserUsername, ApiFactory.UserPassword);

        Assert.Equal(HttpStatusCode.OK, (await SendAsync(HttpMethod.Get, "/api/v1/recurring-payments", token)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await SendAsync(HttpMethod.Get, "/api/v1/payments/summary", token)).StatusCode);
    }

    [Fact]
    public async Task User_CannotCreateOrDeletePayments()
    {
        var token = await LoginAsync(ApiFactory.UserUsername, ApiFactory.UserPassword);
        var payment = await CreatePaymentAsync();

        var create = await SendAsync(
            HttpMethod.Post,
            "/api/v1/recurring-payments",
            token,
            new CreatePaymentDto("Запрещено", 10m, "RUB", DateTime.UtcNow.Date));
        var delete = await SendAsync(HttpMethod.Delete, $"/api/v1/recurring-payments/{payment.Id}", token);

        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task User_CanReadOccurrenceListAndItem()
    {
        var token = await LoginAsync(ApiFactory.UserUsername, ApiFactory.UserPassword);
        var payment = await CreatePaymentAsync();

        var list = await SendAsync(HttpMethod.Get, "/api/v1/payment-occurrences", token);
        var item = await SendAsync(
            HttpMethod.Get,
            $"/api/v1/payment-occurrences/{payment.NextOccurrence!.Id}",
            token);

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var occurrences = await list.Content.ReadFromJsonAsync<List<PaymentOccurrenceResponseDto>>();
        Assert.Contains(occurrences!, o => o.Id == payment.NextOccurrence.Id);

        Assert.Equal(HttpStatusCode.OK, item.StatusCode);
        var occurrence = await item.Content.ReadFromJsonAsync<PaymentOccurrenceResponseDto>();
        Assert.Equal(payment.NextOccurrence.Id, occurrence!.Id);
    }

    [Fact]
    public async Task OccurrenceById_Returns404_ForUnknownOccurrence()
    {
        var token = await LoginAsync(ApiFactory.UserUsername, ApiFactory.UserPassword);

        var response = await SendAsync(
            HttpMethod.Get,
            $"/api/v1/payment-occurrences/{Guid.NewGuid()}",
            token);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task User_CannotManageUsers()
    {
        var token = await LoginAsync(ApiFactory.UserUsername, ApiFactory.UserPassword);

        var list = await SendAsync(HttpMethod.Get, "/api/v1/users", token);
        var create = await SendAsync(
            HttpMethod.Post,
            "/api/v1/users",
            token,
            new CreateUserDto("intruder", "Intruder-Passw0rd!", "User"));
        var update = await SendAsync(
            HttpMethod.Patch,
            $"/api/v1/users/{Guid.NewGuid()}",
            token,
            new UpdateUserDto("(unchanged)", "Admin"));
        var delete = await SendAsync(HttpMethod.Delete, $"/api/v1/users/{Guid.NewGuid()}", token);

        Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task User_CanReadOwnProfileAndLogout()
    {
        var token = await LoginAsync(ApiFactory.UserUsername, ApiFactory.UserPassword);

        var me = await SendAsync(HttpMethod.Get, "/api/v1/auth/me", token);
        var logout = await SendAsync(HttpMethod.Post, "/api/v1/auth/logout", token);

        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        var profile = await me.Content.ReadFromJsonAsync<UserResponseDto>();
        Assert.Equal(ApiFactory.UserUsername, profile!.Username);
        Assert.Equal("User", profile.Role);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
    }

    [Fact]
    public async Task User_CanMarkOccurrencePaid()
    {
        var token = await LoginAsync(ApiFactory.UserUsername, ApiFactory.UserPassword);
        var payment = await CreatePaymentAsync();

        var response = await SendAsync(
            HttpMethod.Patch,
            $"/api/v1/payment-occurrences/{payment.NextOccurrence!.Id}",
            token,
            new UpdatePaymentStatusDto(true, null));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<PaymentOccurrenceResponseDto>();
        Assert.True(updated!.Status == "Paid");
    }

    [Fact]
    public async Task User_CannotUseBotEndpoints()
    {
        var token = await LoginAsync(ApiFactory.UserUsername, ApiFactory.UserPassword);
        var payment = await CreatePaymentAsync();

        var reminders = await SendAsync(HttpMethod.Get, "/api/v1/payment-occurrences/reminders", token);
        var markSent = await SendAsync(
            HttpMethod.Patch,
            $"/api/v1/payment-occurrences/{payment.NextOccurrence!.Id}/reminder-sent",
            token);

        Assert.Equal(HttpStatusCode.Forbidden, reminders.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, markSent.StatusCode);
    }

    [Fact]
    public async Task User_WithJwt_CannotActAsServiceViaHeader()
    {
        // Роль берётся из подписанного JWT: наличие сервисного заголовка не повышает права.
        var token = await LoginAsync(ApiFactory.UserUsername, ApiFactory.UserPassword);

        var response = await SendAsync(
            HttpMethod.Get,
            "/api/v1/payment-occurrences/reminders",
            token,
            serviceToken: ApiFactory.BotToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- Роль Service (Telegram-бот) ----------

    [Fact]
    public async Task Service_CanReadRemindersAndMarkThemSent()
    {
        var payment = await CreatePaymentAsync(dueDate: DateTime.UtcNow.Date.AddDays(1));

        var reminders = await SendAsync(
            HttpMethod.Get,
            "/api/v1/payment-occurrences/reminders",
            serviceToken: ApiFactory.BotToken);

        Assert.Equal(HttpStatusCode.OK, reminders.StatusCode);
        var list = await reminders.Content.ReadFromJsonAsync<List<ReminderOccurrenceDto>>();
        Assert.Contains(list!, r => r.Id == payment.NextOccurrence!.Id);

        var markSent = await SendAsync(
            HttpMethod.Patch,
            $"/api/v1/payment-occurrences/{payment.NextOccurrence!.Id}/reminder-sent",
            serviceToken: ApiFactory.BotToken);
        Assert.Equal(HttpStatusCode.NoContent, markSent.StatusCode);

        var after = await SendAsync(
            HttpMethod.Get,
            "/api/v1/payment-occurrences/reminders",
            serviceToken: ApiFactory.BotToken);
        var afterList = await after.Content.ReadFromJsonAsync<List<ReminderOccurrenceDto>>();
        Assert.DoesNotContain(afterList!, r => r.Id == payment.NextOccurrence!.Id);
    }

    [Fact]
    public async Task Service_CannotAccessUserEndpoints()
    {
        var payment = await CreatePaymentAsync();

        var payments = await SendAsync(HttpMethod.Get, "/api/v1/recurring-payments", serviceToken: ApiFactory.BotToken);
        var analytics = await SendAsync(HttpMethod.Get, "/api/v1/payments/summary", serviceToken: ApiFactory.BotToken);
        var occurrences = await SendAsync(HttpMethod.Get, "/api/v1/payment-occurrences", serviceToken: ApiFactory.BotToken);
        var occurrence = await SendAsync(
            HttpMethod.Get,
            $"/api/v1/payment-occurrences/{payment.NextOccurrence!.Id}",
            serviceToken: ApiFactory.BotToken);
        var status = await SendAsync(
            HttpMethod.Patch,
            $"/api/v1/payment-occurrences/{payment.NextOccurrence!.Id}",
            body: new UpdatePaymentStatusDto(true, null),
            serviceToken: ApiFactory.BotToken);
        var users = await SendAsync(HttpMethod.Get, "/api/v1/users", serviceToken: ApiFactory.BotToken);

        Assert.Equal(HttpStatusCode.Forbidden, payments.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, analytics.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, occurrences.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, occurrence.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, status.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, users.StatusCode);
    }

    // ---------- Роль Admin ----------

    [Fact]
    public async Task Admin_HasAccessToEveryEndpointGroup()
    {
        var token = await LoginAsync(ApiFactory.AdminUsername, ApiFactory.AdminPassword);

        var users = await SendAsync(HttpMethod.Get, "/api/v1/users", token);
        var payments = await SendAsync(HttpMethod.Get, "/api/v1/recurring-payments", token);
        var analytics = await SendAsync(HttpMethod.Get, "/api/v1/payments/summary", token);
        var reminders = await SendAsync(HttpMethod.Get, "/api/v1/payment-occurrences/reminders", token);
        var created = await SendAsync(
            HttpMethod.Post,
            "/api/v1/users",
            token,
            new CreateUserDto($"api-user-{Guid.NewGuid():N}"[..20], "Admin-Created-Passw0rd!", "User"));

        Assert.Equal(HttpStatusCode.OK, users.StatusCode);
        Assert.Equal(HttpStatusCode.OK, payments.StatusCode);
        Assert.Equal(HttpStatusCode.OK, analytics.StatusCode);
        Assert.Equal(HttpStatusCode.OK, reminders.StatusCode);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
    }

    [Fact]
    public async Task Admin_CreatesAndSoftDeletesPayment()
    {
        var token = await LoginAsync(ApiFactory.AdminUsername, ApiFactory.AdminPassword);

        var create = await SendAsync(
            HttpMethod.Post,
            "/api/v1/recurring-payments",
            token,
            new CreatePaymentDto("Админский платёж", 100m, "RUB", DateTime.UtcNow.Date));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<PaymentResponseDto>();
        Assert.NotNull(created!.NextOccurrence);

        var delete = await SendAsync(HttpMethod.Delete, $"/api/v1/recurring-payments/{created.Id}", token);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var readAfterDelete = await SendAsync(HttpMethod.Get, $"/api/v1/recurring-payments/{created.Id}", token);
        Assert.Equal(HttpStatusCode.NotFound, readAfterDelete.StatusCode);
    }

    [Fact]
    public async Task Admin_UpdatesAndDeletesUser()
    {
        var token = await LoginAsync(ApiFactory.AdminUsername, ApiFactory.AdminPassword);
        var username = $"upd-user-{Guid.NewGuid():N}"[..20];

        var created = await SendAsync(
            HttpMethod.Post,
            "/api/v1/users",
            token,
            new CreateUserDto(username, "Initial-Passw0rd!", "User"));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var user = await created.Content.ReadFromJsonAsync<UserResponseDto>();
        var update = await SendAsync(
            HttpMethod.Patch,
            $"/api/v1/users/{user!.Id}",
            token,
            new UpdateUserDto("Changed-Passw0rd!", "User"));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<UserResponseDto>();
        Assert.Equal("User", updated!.Role);
        Assert.Equal(username, updated.Username);

        var delete = await SendAsync(HttpMethod.Delete, $"/api/v1/users/{user.Id}", token);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var afterDelete = await SendAsync(HttpMethod.Get, $"/api/v1/users/{user.Id}", token);
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }

    [Fact]
    public async Task Admin_CannotTouchAnotherAdmin()
    {
        var token = await LoginAsync(ApiFactory.AdminUsername, ApiFactory.AdminPassword);
        var otherAdminId = await GetUserIdAsync(ApiFactory.SecondAdminUsername);

        var update = await SendAsync(
            HttpMethod.Patch,
            $"/api/v1/users/{otherAdminId}",
            token,
            new UpdateUserDto("Hijacked-Passw0rd!", "User"));
        var delete = await SendAsync(HttpMethod.Delete, $"/api/v1/users/{otherAdminId}", token);

        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task Admin_CanChangeOwnPassword()
    {
        var token = await LoginAsync(ApiFactory.SecondAdminUsername, ApiFactory.SecondAdminPassword);
        var selfId = await GetUserIdAsync(ApiFactory.SecondAdminUsername);

        var update = await SendAsync(
            HttpMethod.Patch,
            $"/api/v1/users/{selfId}",
            token,
            new UpdateUserDto("(unchanged)", "Admin"));

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
    }

    [Fact]
    public async Task Admin_CannotCreateOrPromoteAdmin()
    {
        var token = await LoginAsync(ApiFactory.AdminUsername, ApiFactory.AdminPassword);
        var userId = await GetUserIdAsync(ApiFactory.UserUsername);

        var createAdmin = await SendAsync(
            HttpMethod.Post,
            "/api/v1/users",
            token,
            new CreateUserDto($"new-admin-{Guid.NewGuid():N}"[..24], "New-Admin-Passw0rd!", "Admin"));
        var promote = await SendAsync(
            HttpMethod.Patch,
            $"/api/v1/users/{userId}",
            token,
            new UpdateUserDto("(unchanged)", "Admin"));

        Assert.Equal(HttpStatusCode.Forbidden, createAdmin.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, promote.StatusCode);
    }

    [Fact]
    public async Task CreateUser_Returns409_ForDuplicateUsername()
    {
        var token = await LoginAsync(ApiFactory.AdminUsername, ApiFactory.AdminPassword);

        var response = await SendAsync(
            HttpMethod.Post,
            "/api/v1/users",
            token,
            new CreateUserDto(ApiFactory.UserUsername, "Duplicate-Passw0rd!", "User"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task OccurrenceStatus_Returns404_ForUnknownOccurrence()
    {
        var token = await LoginAsync(ApiFactory.UserUsername, ApiFactory.UserPassword);

        var response = await SendAsync(
            HttpMethod.Patch,
            $"/api/v1/payment-occurrences/{Guid.NewGuid()}",
            token,
            new UpdatePaymentStatusDto(true, null));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_Returns400_ForInvalidCurrency()
    {
        var token = await LoginAsync(ApiFactory.AdminUsername, ApiFactory.AdminPassword);

        var response = await SendAsync(
            HttpMethod.Post,
            "/api/v1/recurring-payments",
            token,
            new CreatePaymentDto("Валюта", 10m, "GBP", DateTime.UtcNow.Date));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<PaymentResponseDto> CreatePaymentAsync(DateTime? dueDate = null)
    {
        var token = await LoginAsync(ApiFactory.AdminUsername, ApiFactory.AdminPassword);
        var response = await SendAsync(
            HttpMethod.Post,
            "/api/v1/recurring-payments",
            token,
            new CreatePaymentDto(
                $"Платёж {Guid.NewGuid():N}"[..20],
                42.50m,
                "RUB",
                dueDate ?? DateTime.UtcNow.Date.AddDays(5)));

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PaymentResponseDto>())!;
    }

    private async Task<string> LoginAsync(string username, string password)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { username, password });

        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return auth!.Token;
    }

    private async Task<Guid> GetUserIdAsync(string username)
    {
        var token = await LoginAsync(ApiFactory.AdminUsername, ApiFactory.AdminPassword);
        var response = await SendAsync(HttpMethod.Get, "/api/v1/users", token);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<UserResponseDto>>();
        return users!.Single(u => u.Username == username).Id;
    }

    private Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        string? token = null,
        object? body = null,
        string? serviceToken = null)
    {
        var request = new HttpRequestMessage(method, url);

        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (serviceToken is not null)
        {
            request.Headers.Add("X-Service-Token", serviceToken);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return _client.SendAsync(request);
    }
}



