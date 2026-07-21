using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MvcMovie.Data;
using MvcMovie.Models;
var builder = WebApplication.CreateBuilder(args);

// 1. Get whichever connection string is active (.Development or base)
var connectionString = builder.Configuration.GetConnectionString("MvcMovieContext");

// 2. Configure EF Core to use SQLite exclusively
if (connectionString!.Contains(":memory:"))
{
    // CI/CD Pipeline Mode: Keep the RAM connection open so data doesn't wipe mid-test
    var keepAliveConnection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
    keepAliveConnection.Open();

    builder.Services.AddDbContext<MvcMovieContext>(options =>
        options.UseSqlite(keepAliveConnection));
}
else
{
    // Local Machine Mode: Writes to the local 'MvcMovie.db' file on your hard drive
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
