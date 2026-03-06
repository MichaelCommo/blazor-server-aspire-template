using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Worker;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("appdb")));

// Background worker
builder.Services.AddHostedService<WorkerService>();

var app = builder.Build();

// Status endpoint — Web project calls this via Aspire service discovery
app.MapGet("/api/status", async (AppDbContext db) =>
{
    var count = await db.WorkerHeartbeats.CountAsync();
    var latest = await db.WorkerHeartbeats
        .OrderByDescending(h => h.Timestamp)
        .Select(h => (DateTime?)h.Timestamp)
        .FirstOrDefaultAsync();

    return new { status = "running", timestamp = DateTime.UtcNow, iterations = count, lastHeartbeat = latest };
});

app.MapDefaultEndpoints();

app.Run();
