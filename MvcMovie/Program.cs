using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using MvcMovie.Data;
using MvcMovie.Models;
var builder = WebApplication.CreateBuilder(args);

// 1. Get whichever connection string is active (.Development, base, or Environment Override)
var connectionString = builder.Configuration.GetConnectionString("MvcMovieContext");

// 2. Configure EF Core Dynamically Based on Connection String Contents
if (connectionString != null && connectionString.Contains(":memory:"))
{
    // A. CI/CD INTEGRATION TEST MODE (SQLite In-Memory)
    var keepAliveConnection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
    keepAliveConnection.Open();

    builder.Host.ConfigureServices((context, services) =>
    {
        services.AddSingleton(keepAliveConnection);
    });

    builder.Services.AddDbContext<MvcMovieContext>(options =>
        options.UseSqlite(keepAliveConnection),
        ServiceLifetime.Scoped);
}
else if (connectionString != null && (connectionString.Contains("Server=") || connectionString.Contains("Database=")))
{
    // B. NEW: PRODUCTION & CI/CD FULL-STACK E2E MODE (Real SQL Server Container)
    // When the Playwright workflow injects an MSSQL string, EF Core dynamically swaps engines!
    builder.Services.AddDbContext<MvcMovieContext>(options =>
        options.UseSqlServer(connectionString),
        ServiceLifetime.Scoped);
}
else
{
    // C. LOCAL MACHINE DEVELOPMENT MODE (Local SQLite File)
    builder.Services.AddDbContext<MvcMovieContext>(options =>
        options.UseSqlite(connectionString));
}

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
