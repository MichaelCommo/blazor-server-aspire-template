using Project.Data;
using Project.Data.Entities;

namespace Project.Worker;

public class WorkerService(
    IServiceProvider serviceProvider,
    ILogger<WorkerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.WorkerHeartbeats.Add(new WorkerHeartbeat
                {
                    Timestamp = DateTime.UtcNow
                });
                await db.SaveChangesAsync(stoppingToken);

                logger.LogInformation("Heartbeat recorded at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Failed to record heartbeat.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }

        logger.LogInformation("Worker stopped.");
    }
}
