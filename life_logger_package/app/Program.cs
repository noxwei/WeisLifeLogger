using Microsoft.EntityFrameworkCore;
using LifeLoggerApp.Data;
using LifeLoggerApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add SQLite Database Context
builder.Services.AddDbContext<LifeLoggerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add application services
builder.Services.AddScoped<IJournalService, JournalService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFile("../logs/webapp.log");

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LifeLoggerDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Add API endpoints
app.MapGet("/api/entries", async (IJournalService journalService, int page = 1, int pageSize = 50) =>
{
    var entries = await journalService.GetEntriesAsync(page, pageSize);
    return Results.Ok(entries);
});

app.MapGet("/api/summary/{date}", async (IJournalService journalService, string date) =>
{
    var summary = await journalService.GetDailySummaryAsync(date);
    return Results.Ok(summary);
});

app.MapGet("/api/locations", async (IAnalyticsService analyticsService) =>
{
    var locations = await analyticsService.GetLocationFrequencyAsync();
    return Results.Ok(locations);
});

app.MapGet("/api/search", async (IJournalService journalService, string q) =>
{
    var results = await journalService.SearchEntriesAsync(q);
    return Results.Ok(results);
});

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

Console.WriteLine("Wei Life Logger Web Application v1.0");
Console.WriteLine($"Starting on: {builder.Configuration["urls"] ?? "http://localhost:5000"}");

app.Run();
