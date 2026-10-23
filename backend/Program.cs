using InvoiceKeeper.Infrastructure.Configuration;
using InvoiceKeeper.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

DotEnvLoader.LoadFromDirectoryTree(Directory.GetCurrentDirectory());

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(DatabaseConnectionString.Resolve(builder.Configuration)));

var app = builder.Build();

// Apply pending migrations so `docker compose up` brings up a ready-to-use database.
if (app.Configuration.GetValue("Database:MigrateOnStartup", false))
{
    await app.MigrateDatabaseAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
