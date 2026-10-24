using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InvoiceKeeper.Application.DTOs;

namespace InvoiceKeeper.Tests.Api;

/// <summary>Матрица доступа к HTTP API для ролей Admin, User и Service.</summary>
[Collection(ApiCollection.Name)]
public sealed class ApiAccessTests
{
    private readonly HttpClient _client;

    public ApiAccessTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ---------- Без аутентификации ----------

    [Theory]
    [InlineData("GET", "/api/v1/payments")]
    [InlineData("POST", "/api/v1/payments")]
    [InlineData("GET", "/api/v1/payments/6a1f3c1e-0000-0000-0000-000000000000")]
    [InlineData("DELETE", "/api/v1/payments/6a1f3c1e-0000-0000-0000-000000000000")]
    [InlineData("GET", "/api/v1/analytics/summary")]
    [InlineData("GET", "/api/v1/users")]
    [InlineData("POST", "/api/v1/users")]
    [InlineData("GET", "/api/v1/occurrences/reminders")]
    [InlineData("PATCH", "/api/v1/occurrences/6a1f3c1e-0000-0000-0000-000000000000/status")]
    [InlineData("PATCH", "/api/v1/occurrences/6a1f3c1e-0000-0000-0000-000000000000/reminder-sent")]
    public async Task ProtectedEndpoints_Return401_WithoutToken(string method, string url)
    {
        var response = await SendAsync(new HttpMethod(method), url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Payments_Return401_ForMalformedJwt()
    {
        var response = await SendAsync(HttpMethod.Get, "/api/v1/payments", token: "not-a-jwt");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Reminders_Return401_ForWrongServiceToken()
    {
        var response = await SendAsync(
            HttpMethod.Get,
            "/api/v1/occurrences/reminders",
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

    // ---------- Роль User ----------

    [Fact]
    public async Task User_CanReadPaymentsAndAnalytics()
    {
        var token = await LoginAsync(ApiFactory.UserUsername, ApiFactory.UserPassword);

        Assert.Equal(HttpStatusCode.OK, (await SendAsync(HttpMethod.Get, "/api/v1/payments", token)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await SendAsync(HttpMethod.Get, "/api/v1/analytics/summary", token)).StatusCode);
    }

    [Fact]
    public async Task User_CannotCreateOrDeletePayments()
    {
        var token = await LoginAsync(ApiFactory.UserUsername, ApiFactory.UserPassword);
        var payment = await CreatePaymentAsync();

        var create = await SendAsync(
            HttpMethod.Post,
            "/api/v1/payments",
            token,
            new CreatePaymentDto("Запрещено", 10m, "RUB", DateTime.UtcNow.Date));
        var delete = await SendAsync(HttpMethod.Delete, $"/api/v1/payments/{payment.Id}", token);

        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
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
            new CreateUserDto("intruder", "Intruder-Passw0rd!", "Admin"));

        Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
    }

    [Fact]
    public async Task User_CanMarkOccurrencePaid()
    {
        var token = await LoginAsync(ApiFactory.UserUsername, ApiFactory.UserPassword);
        var payment = await CreatePaymentAsync();

        var response = await SendAsync(
            HttpMethod.Patch,
            $"/api/v1/occurrences/{payment.NextOccurrence!.Id}/status",
            token,
            new UpdatePaymentStatusDto(true, null));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<PaymentOccurrenceResponseDto>();
        Assert.True(updated!.IsPaid);
    }

    [Fact]
    public async Task User_CannotUseBotEndpoints()
    {
        var token = await LoginAsync(ApiFactory.UserUsername, ApiFactory.UserPassword);
        var payment = await CreatePaymentAsync();

        var reminders = await SendAsync(HttpMethod.Get, "/api/v1/occurrences/reminders", token);
        var markSent = await SendAsync(
            HttpMethod.Patch,
            $"/api/v1/occurrences/{payment.NextOccurrence!.Id}/reminder-sent",
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
            "/api/v1/occurrences/reminders",
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
            "/api/v1/occurrences/reminders",
            serviceToken: ApiFactory.BotToken);

        Assert.Equal(HttpStatusCode.OK, reminders.StatusCode);
        var list = await reminders.Content.ReadFromJsonAsync<List<ReminderOccurrenceDto>>();
        Assert.Contains(list!, r => r.Id == payment.NextOccurrence!.Id);

        var markSent = await SendAsync(
            HttpMethod.Patch,
            $"/api/v1/occurrences/{payment.NextOccurrence!.Id}/reminder-sent",
            serviceToken: ApiFactory.BotToken);
        Assert.Equal(HttpStatusCode.NoContent, markSent.StatusCode);

        var after = await SendAsync(
            HttpMethod.Get,
            "/api/v1/occurrences/reminders",
            serviceToken: ApiFactory.BotToken);
        var afterList = await after.Content.ReadFromJsonAsync<List<ReminderOccurrenceDto>>();
        Assert.DoesNotContain(afterList!, r => r.Id == payment.NextOccurrence!.Id);
    }

    [Fact]
    public async Task Service_CannotAccessUserEndpoints()
    {
        var payment = await CreatePaymentAsync();

        var payments = await SendAsync(HttpMethod.Get, "/api/v1/payments", serviceToken: ApiFactory.BotToken);
        var analytics = await SendAsync(HttpMethod.Get, "/api/v1/analytics/summary", serviceToken: ApiFactory.BotToken);
        var status = await SendAsync(
            HttpMethod.Patch,
            $"/api/v1/occurrences/{payment.NextOccurrence!.Id}/status",
            body: new UpdatePaymentStatusDto(true, null),
            serviceToken: ApiFactory.BotToken);
        var users = await SendAsync(HttpMethod.Get, "/api/v1/users", serviceToken: ApiFactory.BotToken);

        Assert.Equal(HttpStatusCode.Forbidden, payments.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, analytics.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, status.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, users.StatusCode);
    }

    // ---------- Роль Admin ----------

    [Fact]
    public async Task Admin_HasAccessToEveryEndpointGroup()
    {
        var token = await LoginAsync(ApiFactory.AdminUsername, ApiFactory.AdminPassword);

        var users = await SendAsync(HttpMethod.Get, "/api/v1/users", token);
        var payments = await SendAsync(HttpMethod.Get, "/api/v1/payments", token);
        var analytics = await SendAsync(HttpMethod.Get, "/api/v1/analytics/summary", token);
        var reminders = await SendAsync(HttpMethod.Get, "/api/v1/occurrences/reminders", token);
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
            "/api/v1/payments",
            token,
            new CreatePaymentDto("Админский платёж", 100m, "RUB", DateTime.UtcNow.Date));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<PaymentResponseDto>();
        Assert.NotNull(created!.NextOccurrence);

        var delete = await SendAsync(HttpMethod.Delete, $"/api/v1/payments/{created.Id}", token);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var readAfterDelete = await SendAsync(HttpMethod.Get, $"/api/v1/payments/{created.Id}", token);
        Assert.Equal(HttpStatusCode.NotFound, readAfterDelete.StatusCode);
    }

    [Fact]
    public async Task OccurrenceStatus_Returns404_ForUnknownOccurrence()
    {
        var token = await LoginAsync(ApiFactory.UserUsername, ApiFactory.UserPassword);

        var response = await SendAsync(
            HttpMethod.Patch,
            $"/api/v1/occurrences/{Guid.NewGuid()}/status",
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
            "/api/v1/payments",
            token,
            new CreatePaymentDto("Валюта", 10m, "GBP", DateTime.UtcNow.Date));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<PaymentResponseDto> CreatePaymentAsync(DateTime? dueDate = null)
    {
        var token = await LoginAsync(ApiFactory.AdminUsername, ApiFactory.AdminPassword);
        var response = await SendAsync(
            HttpMethod.Post,
            "/api/v1/payments",
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
