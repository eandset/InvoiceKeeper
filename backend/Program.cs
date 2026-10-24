using InvoiceKeeper.Api.Configuration;
using InvoiceKeeper.Api.OpenApi;
using InvoiceKeeper.Api.Middleware;
using InvoiceKeeper.Application.Services;
using InvoiceKeeper.Infrastructure.Configuration;
using InvoiceKeeper.Infrastructure.Data;
using InvoiceKeeper.Infrastructure.Data.Seed;
using InvoiceKeeper.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

DotEnvLoader.LoadFromDirectoryTree(Directory.GetCurrentDirectory());

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(DatabaseConnectionString.Resolve(builder.Configuration)));

var jwtOptions = JwtOptionsResolver.Resolve(builder.Configuration, builder.Environment);

builder.Services.Configure<JwtOptions>(options =>
{
    options.Secret = jwtOptions.Secret;
    options.Issuer = jwtOptions.Issuer;
    options.Audience = jwtOptions.Audience;
    options.ExpiryInHours = jwtOptions.ExpiryInHours;
});

builder.Services.Configure<BotAuthOptions>(options =>
{
    var section = builder.Configuration.GetSection(BotAuthOptions.SectionName);
    var envToken = builder.Configuration["BOT_SERVICE_TOKEN"];
    options.Token = envToken ?? section["Token"] ?? string.Empty;
    options.ServiceName = section["ServiceName"] ?? options.ServiceName;
});

builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<AdminSeeder>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<OverdueRecalculationService>();
builder.Services.AddHostedService<OverdueRecalculationHostedService>();

builder.Services.AddScoped<TelegramAuthService>();

builder.Services.AddApiDocumentation();
builder.Services.AddWebClientCors(builder.Configuration, builder.Environment);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = "Multi";
        options.DefaultAuthenticateScheme = "Multi";
        options.DefaultChallengeScheme = "Multi";
    })
    .AddPolicyScheme("Multi", "JWT or service token", options =>
    {
        options.ForwardDefaultSelector = context =>
            !context.Request.Headers.ContainsKey("Authorization") &&
            context.Request.Headers.ContainsKey(ServiceTokenAuthenticationHandler.HeaderName)
                ? ServiceTokenAuthenticationHandler.SchemeName
                : JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    })
    .AddScheme<AuthenticationSchemeOptions, ServiceTokenAuthenticationHandler>(
        ServiceTokenAuthenticationHandler.SchemeName, _ => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("Admin", "User"));
    options.AddPolicy("ServiceOrAdmin", policy => policy.RequireRole("Admin", ServiceTokenAuthenticationHandler.ServiceRole));
});

var app = builder.Build();

app.UseMiddleware<ApiExceptionHandlingMiddleware>();

// Apply pending migrations so `docker compose up` brings up a ready-to-use database.
if (app.Configuration.GetValue("Database:MigrateOnStartup", false))
{
    await app.MigrateDatabaseAsync();
}

if (app.Configuration.GetValue("Database:SeedOnStartup", true))
{
    await app.SeedAdminAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// В контейнере TLS обычно завершается на reverse proxy, поэтому редирект можно отключить.
if (app.Configuration.GetValue("HttpsRedirection:Enabled", true))
{
    app.UseHttpsRedirection();
}
app.UseRouting();

app.UseCors(CorsConfiguration.PolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseApiDocumentation();

app.MapStaticAssets();

app.MapControllers();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

/// <summary>Точка входа доступна интеграционным тестам через WebApplicationFactory.</summary>
public partial class Program
{
}

